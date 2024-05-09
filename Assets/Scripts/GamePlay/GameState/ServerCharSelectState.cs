using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class ServerCharSelectState : GameStateBehaviour
{
    public override GameState ActiveState => GameState.CharSelect;

    [SerializeField]
    NetcodeHooks m_NetcodeHooks;

    public NetworkCharSelection networkCharSelection { get; private set; }

    Coroutine m_WaitToEndLobbyCoroutine;

    [Inject]
    ConnectionManager m_ConnectionManager;

    protected override void Awake()
    {
        base.Awake();
        networkCharSelection = GetComponent<NetworkCharSelection>();

        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_NetcodeHooks)
        {
            m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
        }
    }
    private void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            enabled = false;
        }
        else
        {
            networkCharSelection.OnClientChangedSeat += OnClientChangedSeat;
        }
    }

    private void OnNetworkDespawn()
    {
        if (networkCharSelection)
        {
            networkCharSelection.OnClientChangedSeat -= OnClientChangedSeat;
        };
    }

    private void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
    {
        CloseLobbyIfReady();
    }
    void CloseLobbyIfReady()
    {
        m_WaitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
    }
    IEnumerator WaitToEndLobby()
    {
        yield return new WaitForSeconds(3);
        SceneLoaderWrapper.Instance.LoadScene("Game01", useNetworkSceneManager: true);
    }
}
