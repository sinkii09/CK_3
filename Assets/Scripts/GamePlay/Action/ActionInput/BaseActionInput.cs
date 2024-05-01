using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseActionInput : MonoBehaviour
{
    protected ServerCharacter m_PlayerOwner;
    protected Vector3 m_Origin;
    protected ActionID m_ActionID;
    protected Action<ActionRequestData> m_SendInput;
    System.Action m_OnFinished;
    public void Initiate(ServerCharacter owner,Vector3 origin,ActionID actionID, Action<ActionRequestData> onSendInput, System.Action onFinished)
    {
        m_PlayerOwner = owner;
        m_Origin = origin;
        m_ActionID = actionID;
        m_SendInput = onSendInput;
        m_OnFinished = onFinished;
    }
    public void OnDestroy()
    {
        m_OnFinished();
    }
    public virtual void OnReleaseKey()
    {

    }
}
