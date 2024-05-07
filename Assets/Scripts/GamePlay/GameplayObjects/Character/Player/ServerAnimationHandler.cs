using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerAnimationHandler : NetworkBehaviour
{
    [SerializeField]
    NetworkAnimator m_NetworkAnimator;

    [SerializeField]
    VisualizationConfiguration m_VisualizationConfiguration;

    [SerializeField]
    NetworkLifeState m_NetworkLifeState;

    public NetworkAnimator NetworkAnimator => m_NetworkAnimator;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(WaitToRegisterOnLifeStateChanged());
        }

    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            m_NetworkLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
        }
    }
    IEnumerator WaitToRegisterOnLifeStateChanged()
    {
        yield return new WaitForEndOfFrame();
        m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
        if (m_NetworkLifeState.LifeState.Value != LifeState.Alive)
        {
            OnLifeStateChanged(LifeState.Alive, m_NetworkLifeState.LifeState.Value);
        }
    }
    void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
    {
        switch (newValue)
        {
            case LifeState.Alive:
                NetworkAnimator.SetTrigger(m_VisualizationConfiguration.AliveStateTriggerID);
                break;
            case LifeState.Fainted:
                NetworkAnimator.SetTrigger(m_VisualizationConfiguration.FaintedStateTriggerID);
                break;
            case LifeState.Dead:
                NetworkAnimator.SetTrigger(m_VisualizationConfiguration.DeadStateTriggerID);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
        }
    }


}
