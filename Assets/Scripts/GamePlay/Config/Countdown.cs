using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Countdown : NetworkBehaviour
{
    private float startTime;
    [SerializeField]
    private float countdownRuration = 10f;

    private bool isStartCountdown;


    public event System.Action PlayTimeExpired;
    public void StartCountdown()
    {
        isStartCountdown = true;
        startTime = NetworkManager.LocalTime.TimeAsFloat;
    }
    private void Update()
    {
        if (IsServer)
        {
            if (!isStartCountdown) { return; }
            float elapsedTime = NetworkManager.LocalTime.TimeAsFloat - startTime;

            float remainingTime = Mathf.Max(0, countdownRuration - elapsedTime);

            RemainingTimeServerRpc(remainingTime);
        }
    }
    [ServerRpc]
    private void RemainingTimeServerRpc(float remainingTime)
    {
        //TODO
        if (remainingTime > 0) return;
        PlayTimeExpired?.Invoke();
    }
}
