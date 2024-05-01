using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedActionInput : BaseActionInput
{
    protected float m_StartTime;

    private void Start()
    {
        transform.position = m_Origin;
        m_StartTime = Time.time;

        var data = new ActionRequestData
        {
            Position = transform.position,
            ActionID = m_ActionID,
            ShouldQueue = false,
            TargetIds = null
        };
        m_SendInput(data);
    }
    public override void OnReleaseKey()
    {
        m_PlayerOwner.RecvStopChargingUpServerRpc();
        Destroy(gameObject);
    }
}
