using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ServerActionPlayer 
{
    private ServerCharacter m_ServerCharacter;

    private ServerCharacterMovement m_Movement;

    private List<Action> m_Queue;

    private List<Action> m_NonBlockingActions;

    private Dictionary<ActionID, float> m_LastUsedTimestamps;

    private const float k_MaxQueueTimeDepth = 1.6f;

    private ActionRequestData m_PendingSynthesizedAction = new ActionRequestData();
    private bool m_HasPendingSynthesizedAction;

    // get the count of currently running actions.
    public int RunningActionCount
    {
        get
        {
            return m_NonBlockingActions.Count + (m_Queue.Count > 0 ? 1 : 0);
        }
    }

    public ServerActionPlayer(ServerCharacter serverCharacter)
    {
        m_ServerCharacter = serverCharacter;
        m_Movement = serverCharacter.Movement;
        m_Queue = new List<Action>();
        m_NonBlockingActions = new List<Action>();
        m_LastUsedTimestamps = new Dictionary<ActionID, float>();
    }

    /// <summary>
    /// Add a new action to the queue and start it if it’s the only action.
    /// </summary>
    public void PlayAction(ref ActionRequestData action)
    {
        // If the action should not be queued and there is an interruptible action at the front of the queue,
        // or the new action can interrupt the one at the front, clear the actions.
        if (!action.ShouldQueue && m_Queue.Count > 0 &&
                (m_Queue[0].Config.ActionInterruptible ||
                    m_Queue[0].Config.CanBeInterruptedBy(action.ActionID)))
        {
            ClearActions(false);
        }
        // If the estimated time to execute the queued actions exceeds the maximum allowed, discard the new action.
        if (GetQueueTimeDepth() >= k_MaxQueueTimeDepth)
        {
            return;
        }
        // Create a new action from the provided data and add it to the queue.
        var newAction = ActionFactory.CreateActionFromData(ref action);
        m_Queue.Add(newAction);

        // If the queue was empty before adding the new action, it starts the action.
        if (m_Queue.Count == 1) { StartAction(); }

    }

    /// <summary>
    /// Clear the action queue and optionally the non-blocking actions.
    /// </summary>
    public void ClearActions(bool cancelNonBlocking)
    {
        // If there are actions in the queue, cancel the first one and remove its timestamp.
        if (m_Queue.Count > 0)
        {
            m_LastUsedTimestamps.Remove(m_Queue[0].ActionID);
            m_Queue[0].Cancel(m_ServerCharacter);
        }
        // Clear the action queue and return the actions to a pool for reuse.
        {
            var removedActions = ListPool<Action>.Get();
            foreach (var action in m_Queue)
            {
                removedActions.Add(action);
            }
            m_Queue.Clear();
            foreach (var action in removedActions)
            {
                TryReturnAction(action);
            }
            ListPool<Action>.Release(removedActions);
        }

        //If requested, cancel non-blocking actions as well and return them to the pool.
        if (cancelNonBlocking)
        {
            var removedActions = ListPool<Action>.Get();
            foreach (var action in m_NonBlockingActions)
            {
                action.Cancel(m_ServerCharacter);
                removedActions.Add(action);
            }
            m_NonBlockingActions.Clear();
            foreach (var action in removedActions)
            {
                TryReturnAction(action);
            }
            ListPool<Action>.Release(removedActions);
        }
    }

    /// <summary>
    /// Retrieve the data of the currently active action.
    /// </summary>
    public bool GetActiveActionInfo(out ActionRequestData data)
    {
        //If there’s at least one action in the queue, it returns the data of the first action and
        if (m_Queue.Count > 0)
        {
            data = m_Queue[0].Data;
            return true;
        }
        //If the queue is empty, it returns a new ActionRequestData instance
        else
        {
            data = new ActionRequestData();
            return false;
        }
    }

    /// <summary>
    /// check if enough time has passed to reuse an action.
    /// </summary>
    public bool IsReuseTimeElapsed(ActionID actionID)
    {
        if (m_LastUsedTimestamps.TryGetValue(actionID, out float lastTimeUsed))
        {
            var abilityConfig = GameDataSource.Instance.GetActionPrototypeByID(actionID).Config;

            float reuseTime = abilityConfig.ReuseTimeSeconds;
            if (reuseTime > 0 && Time.time - lastTimeUsed < reuseTime)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Start the first action in the queue if it’s ready to be executed.
    /// </summary>
    private void StartAction()
    {
        if (m_Queue.Count > 0)
        {
            #region checks if the first action in the queue has a reuse time and if the current time is within that reuse time since the last use. If so, it advances the queue without starting the action
            // Retrieve the reuse time for the first action in the queue.
            float reuseTime = m_Queue[0].Config.ReuseTimeSeconds;

            // Check if the action has a reuse time and if it has been used too recently.
            if (reuseTime > 0
                && m_LastUsedTimestamps.TryGetValue(m_Queue[0].ActionID, out float lastTimeUsed)
                && Time.time - lastTimeUsed < reuseTime)
            {
                AdvanceQueue(false);
                return;
            }
            #endregion
            int index = SynthesizeTargetIfNecessary(0);
            SynthesizeChaseIfNecessary(index);

            m_Queue[0].TimeStarted = Time.time;
            bool play = m_Queue[0].OnStart(m_ServerCharacter);
            if(!play)
            {
                AdvanceQueue(false);
                return;
            }
            if (m_Queue[0].Config.ActionInterruptible && !m_Movement.IsPerformingForcedMovement())
            {
                m_Movement.CancelMove();
            }
            m_LastUsedTimestamps[m_Queue[0].ActionID] = Time.time;
            if (m_Queue[0].Config.ExecTimeSeconds == 0 && m_Queue[0].Config.BlockingMode == BlockingModeType.OnlyDuringExecTime)
            {
                m_NonBlockingActions.Add(m_Queue[0]);
                AdvanceQueue(false);
                return;
            }
        }
    }
    /// <summary>
    /// add a chase action to the queue if the current action requires the character to move closer to a target.
    /// </summary>
    /// <param name="baseIndex"></param>
    /// <returns> The new index of the Action being operated on.</returns>
    private int SynthesizeChaseIfNecessary(int baseIndex)
    {
        Action baseAction = m_Queue[baseIndex];

        if (baseAction.Data.ShouldClose && baseAction.Data.TargetIds != null)
        {
            ActionRequestData data = new ActionRequestData
            {
                ActionID = GameDataSource.Instance.GeneralChaseActionPrototype.ActionID,
                TargetIds = baseAction.Data.TargetIds,
                Amount = baseAction.Config.Range
            };
            baseAction.Data.ShouldClose = false; //you only get to do this once!
            Action chaseAction = ActionFactory.CreateActionFromData(ref data);
            m_Queue.Insert(baseIndex, chaseAction);
            return baseIndex + 1;
        }
        return baseIndex;
    }

    /// <summary>
    /// insert a target action into the queue if the current action is targeted and requires a different target than the character’s current one.
    /// </summary>
    /// <param name="baseIndex"></param>
    /// <returns></returns>
    private int SynthesizeTargetIfNecessary(int baseIndex)
    {
        Action baseAction = m_Queue[baseIndex];
        var targets = baseAction.Data.TargetIds;

        if (targets != null &&
            targets.Length == 1 &&
            targets[0] != m_ServerCharacter.TargetId.Value)
        {
            ActionRequestData data = new ActionRequestData
            {
                ActionID = GameDataSource.Instance.GeneralTargetActionPrototype.ActionID,
                TargetIds = baseAction.Data.TargetIds
            };
            Action targetAction = ActionFactory.CreateActionFromData(ref data);
            m_Queue.Insert(baseIndex, targetAction);
            return baseIndex + 1;
        }
        return baseIndex;
    }
    private void TryReturnAction(Action action)
    {
        if (m_Queue.Contains(action))
        {
            return;
        }

        if (m_NonBlockingActions.Contains(action))
        {
            return;
        }

        ActionFactory.ReturnAction(action);
    }

    /// <summary>
    /// advancing the action queue, ending the current action if necessary, and starting the next action.
    /// </summary>
    /// <param name="endRemoved"> determines whether the current action at the front of the queue should have its End method called </param>
    private void AdvanceQueue(bool endRemoved)
    {
        if (m_Queue.Count > 0)
        {
            if (endRemoved)
            {
                m_Queue[0].End(m_ServerCharacter);

                // if the current action can chain into a new action, a synthesized action is prepared
                if (m_Queue[0].ChainIntoNewAction(ref m_PendingSynthesizedAction))
                {
                    m_HasPendingSynthesizedAction = true;
                }
            }
            var action = m_Queue[0];
            m_Queue.RemoveAt(0);
            TryReturnAction(action);
        }

        // start next action or pending action
        if (!m_HasPendingSynthesizedAction || m_PendingSynthesizedAction.ShouldQueue)
        {
            StartAction();
        }
    }
    public void OnUpdate()
    {
        // Check for any pending synthesized actions and play them.
        if (m_HasPendingSynthesizedAction)
        {
            m_HasPendingSynthesizedAction = false;
            PlayAction(ref m_PendingSynthesizedAction);
        }
        // Move actions from the blocking queue to the non-blocking queue if they are no longer blocking.
        if (m_Queue.Count > 0 && m_Queue[0].ShouldBecomeNonBlocking())
        {
            m_NonBlockingActions.Add(m_Queue[0]);
            AdvanceQueue(false);
        }
        // Update the current blocking action, if any.
        if (m_Queue.Count > 0)
        {
            if (!UpdateAction(m_Queue[0]))
            {
                AdvanceQueue(true);
            }
        }
        // Update all non-blocking actions, removing any that have completed.
        for (int i = m_NonBlockingActions.Count - 1; i >= 0; --i)
        {
            Action runningAction = m_NonBlockingActions[i];
            if (!UpdateAction(runningAction))
            {
                runningAction.End(m_ServerCharacter);
                m_NonBlockingActions.RemoveAt(i);
                TryReturnAction(runningAction);
            }
        }
    }

    /// <summary>
    /// determines whether an action should continue running or if it has expired.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private bool UpdateAction(Action action)
    {
        bool keepGoing = action.OnUpdate(m_ServerCharacter);
        bool expirable = action.Config.DurationSeconds > 0f;
        var timeElapsed = Time.time - action.TimeStarted;
        bool timeExpired = expirable && timeElapsed >= action.Config.DurationSeconds;
        return keepGoing && !timeExpired;
    }
    public virtual void OnGameplayActivity(Action.GameplayActivity activityThatOccurred)
    {
        if (m_Queue.Count > 0)
        {
            m_Queue[0].OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
        }
        foreach (var action in m_NonBlockingActions)
        {
            action.OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
        }
    }
    private float GetQueueTimeDepth()
    {
        if (m_Queue.Count == 0) { return 0; }

        float totalTime = 0;
        foreach (var action in m_Queue)
        {
            var info = action.Config;
            float actionTime = info.BlockingMode == BlockingModeType.OnlyDuringExecTime ? info.ExecTimeSeconds :
                                info.BlockingMode == BlockingModeType.EntireDuration ? info.DurationSeconds :
                                throw new System.Exception($"Unrecognized blocking mode: {info.BlockingMode}");
            totalTime += actionTime;
        }

        return totalTime - m_Queue[0].TimeRunning;
    }
    public void CollisionEntered(Collision collision)
    {
        if (m_Queue.Count > 0)
        {
            m_Queue[0].CollisionEntered(m_ServerCharacter, collision);
        }
    }
    public float GetBuffedValue(Action.BuffableValue buffType)
    {
        float buffedValue = Action.GetUnbuffedValue(buffType);
        if (m_Queue.Count > 0)
        {
            m_Queue[0].BuffValue(buffType, ref buffedValue);
        }
        foreach (var action in m_NonBlockingActions)
        {
            action.BuffValue(buffType, ref buffedValue);
        }
        return buffedValue;
    }
    public void CancelRunningActionsByLogic(ActionLogic logic, bool cancelAll, Action exceptThis = null)
    {
        for (int i = m_NonBlockingActions.Count - 1; i >= 0; --i)
        {
            var action = m_NonBlockingActions[i];
            if (action.Config.Logic == logic && action != exceptThis)
            {
                action.Cancel(m_ServerCharacter);
                m_NonBlockingActions.RemoveAt(i);
                TryReturnAction(action);
                if (!cancelAll) { return; }
            }
        }

        if (m_Queue.Count > 0)
        {
            var action = m_Queue[0];
            if (action.Config.Logic == logic && action != exceptThis)
            {
                action.Cancel(m_ServerCharacter);
                m_Queue.RemoveAt(0);
                TryReturnAction(action);
            }
        }
    }
}
