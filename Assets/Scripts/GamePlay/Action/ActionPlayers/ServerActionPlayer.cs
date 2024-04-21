using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public ServerActionPlayer(ServerCharacter serverCharacter)
    {
        m_ServerCharacter = serverCharacter;
        m_Movement = serverCharacter.Movement;
        m_Queue = new List<Action>();
        m_NonBlockingActions = new List<Action>();
        m_LastUsedTimestamps = new Dictionary<ActionID, float>();
    }
    public void PlayAction(ref ActionRequestData actionData)
    {
        var newAction = ActionFactory.CreateActionFromData(ref actionData);
        m_Queue.Add(newAction);
        StartAction();

    }
    public void ClearActions(bool cancelNonBlocking)
    {

    }
    private void StartAction()
    {
        m_Queue[0].TimeStarted = Time.time;
        bool play = m_Queue[0].OnStart(m_ServerCharacter);
    }
    private void TryReturnAction(Action action)
    {

    }
    private void AdvanceQueue(bool endRemoved)
    {
        var action = m_Queue[0];
        m_Queue.RemoveAt(0);
        TryReturnAction(action);
        if(!m_HasPendingSynthesizedAction || m_PendingSynthesizedAction.ShouldQueue)
        StartAction();
    }
    public void OnUpdate()
    {
        //if(m_HasPendingSynthesizedAction)
        //{
        //    m_HasPendingSynthesizedAction = false;
        //    PlayAction(ref m_PendingSynthesizedAction);
        //}
        //PlayAction(ref m_PendingSynthesizedAction);

        if (m_Queue.Count != 0)
        {
            UpdateAction(m_Queue[0]);
            m_Queue.RemoveAt(0);
        }

    }
    private bool UpdateAction(Action action)
    {
        bool keepGoing = action.OnUpdate(m_ServerCharacter);
        return keepGoing;
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

}
