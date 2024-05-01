using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ClientActionPlayer 
{
    private const float k_AnticipationTimeoutSeconds = 1;


    private List<Action> m_PlayingActions = new List<Action>();

    public ClientCharacter ClientCharacter { get; private set; }

    public ClientActionPlayer(ClientCharacter clientCharacter)
    {
        ClientCharacter = clientCharacter;
    }

    public void OnUpdate()
    {
        for (int i = m_PlayingActions.Count - 1; i >= 0; --i)
        {
            var action = m_PlayingActions[i];
            bool keepGoing = action.AnticipatedClient || action.OnUpdateClient(ClientCharacter);
            bool expirable = action.Config.DurationSeconds > 0f;
            bool timeExpired = expirable && action.TimeRunning >= action.Config.DurationSeconds;
            bool timedOut = action.AnticipatedClient && action.TimeRunning >= k_AnticipationTimeoutSeconds;
            if (!keepGoing || timeExpired || timedOut)
            {
                {
                    if (timedOut) { action.CancelClient(ClientCharacter); } 
                    else { action.EndClient(ClientCharacter); }

                    m_PlayingActions.RemoveAt(i);
                    ActionFactory.ReturnAction(action);
                }
            }
        }

    }
    public void PlayAction(ref ActionRequestData data)
    {
        var anticipatedActionIndex = FindAction(data.ActionID, true);

        var actionFX = anticipatedActionIndex >= 0 ? m_PlayingActions[anticipatedActionIndex] : ActionFactory.CreateActionFromData(ref data);
        if (actionFX.OnStartClient(ClientCharacter))
        {
            if (anticipatedActionIndex < 0)
            {
                m_PlayingActions.Add(actionFX);
            }
            else if(anticipatedActionIndex >= 0)
            {
                var removedAction = m_PlayingActions[anticipatedActionIndex];
                m_PlayingActions.RemoveAt(anticipatedActionIndex);
                ActionFactory.ReturnAction(removedAction);
            }
        }
    }

    private int FindAction(ActionID actionID, bool anticipatedOnly)
    {
        return m_PlayingActions.FindIndex(a => a.ActionID == actionID && (!anticipatedOnly || a.AnticipatedClient));
    }
    public void OnAnimEvent(string id)
    {
        foreach (var actionFX in m_PlayingActions)
        {
            actionFX.OnAnimEventClient(ClientCharacter, id);
        }
    }
    public void OnStoppedChargingUp(float finalChargeUpPercentage)
    {
        foreach (var actionFX in m_PlayingActions)
        {
            actionFX.OnStoppedChargingUpClient(ClientCharacter, finalChargeUpPercentage);
        }
    }
    public void CancelAllActions() 
    {
        foreach (Action action in m_PlayingActions)
        {
            action.CancelClient(ClientCharacter);
            ActionFactory.ReturnAction(action);
        }
        m_PlayingActions.Clear();
    }
    public void CancelAllActionsWithSamePrototypeID(ActionID actionID)
    {
        for (int i = m_PlayingActions.Count - 1; i >= 0; --i)
        {
            if (m_PlayingActions[i].ActionID == actionID)
            {
                var action = m_PlayingActions[i];
                action.CancelClient(ClientCharacter);
                m_PlayingActions.RemoveAt(i);
                ActionFactory.ReturnAction(action);
            }
        }
    }

    public void AnticipateAction(ref ActionRequestData data)
    {
        if(!ClientCharacter.IsAnimating())
        {
            var actionFX = ActionFactory.CreateActionFromData(ref data);
            actionFX.AnticipateActionClient(ClientCharacter);
            m_PlayingActions.Add(actionFX);

        }
    }
}
