using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

public class NetworkHealthState : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<int> HitPoints = new NetworkVariable<int>();

    // public subscribable event to be invoked when HP has been fully depleted
    public event System.Action HitPointsDepleted;

    // public subscribable event to be invoked when HP has been replenished
    public event System.Action HitPointsReplenished;

    void OnEnable()
    {
        HitPoints.OnValueChanged += HitPointsChanged;
    }

    void OnDisable()
    {
        HitPoints.OnValueChanged -= HitPointsChanged;
    }

    void HitPointsChanged(int previousValue, int newValue)
    {
        if (previousValue > 0 && newValue <= 0)
        {
            // newly reached 0 HP
            HitPointsDepleted?.Invoke();
        }
        else if (previousValue <= 0 && newValue > 0)
        {
            // newly revived
            HitPointsReplenished?.Invoke();
        }
    }
}
