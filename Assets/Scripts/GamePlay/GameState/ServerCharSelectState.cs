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

            NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        }
    }

    private void OnNetworkDespawn()
    {
        if(NetworkManager.Singleton)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
        }
        if (networkCharSelection)
        {
            networkCharSelection.OnClientChangedSeat -= OnClientChangedSeat;
        }
    }
    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        if(sceneEvent.SceneEventType != SceneEventType.LoadComplete) { return; }

        SeatNewPlayer(sceneEvent.ClientId);
    }
    
    private void SeatNewPlayer(ulong clientId)
    {
        //TODO
        networkCharSelection.LobbyPlayers.Add(clientId);
    }

    private void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
    {
        CloseLobbyIfReady();
    }
    void CloseLobbyIfReady()
    {
        SaveLobbyResults();
        m_WaitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
    }

    private void SaveLobbyResults()
    {
        //TODO
        foreach(var player in networkCharSelection.LobbyPlayers)
        {
            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(player);

            if(playerNetworkObject && playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer))
            {
                persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value = networkCharSelection.AvatarConfiguration[0].Guid.ToNetworkGuid();
                
            }
        }
    }

    IEnumerator WaitToEndLobby()
    {
        yield return new WaitForSeconds(3);
        SceneLoaderWrapper.Instance.LoadScene("Game01", useNetworkSceneManager: true);
    }
}
