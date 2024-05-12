using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

public class ClientCharSelectState : GameStateBehaviour
{
    public static ClientCharSelectState Instance { get; private set; }
    public override GameState ActiveState { get { return GameState.CharSelect; } }

    #region Components

    [SerializeField]
    NetcodeHooks m_NetcodeHooks;


    [SerializeField]
    NetworkCharSelection m_NetworkCharSelection;

    #endregion
    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
    }

    protected override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnDestroy();
    }
    void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsClient)
        {
            enabled = false;
        }
    }

    void OnNetworkDespawn()
    {

    }

    public void OnPlayerClickedReady()
    {
        if (m_NetworkCharSelection.IsSpawned)
        {
            m_NetworkCharSelection.ChangeSeatServerRPC(NetworkManager.Singleton.LocalClientId, 0, true);
        }
    }
}
