using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using Unity.Services.Authentication;
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
        networkCharSelection.LobbyPlayers.Add(new NetworkCharSelection.PlayerStatus(clientId,(int)clientId, NetworkCharSelection.SeatState.Inactive));
    }
    /// <summary>
    /// Call moi khi moi client chon seat hoac click ready
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="newSeatIdx"></param>
    /// <param name="lockedIn"></param>
    private void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
    {   
        int idx = FindLobbyPlayerIdx(clientId);
        // neu client == null
        if ( idx == -1)
        {
            return;
        }

        if(networkCharSelection.IsLobbyClosed.Value)
        {
            return;
        }
        if(newSeatIdx == -1)
        {
            lockedIn = false;
        }

        networkCharSelection.LobbyPlayers[idx] = new NetworkCharSelection.PlayerStatus(clientId, networkCharSelection.LobbyPlayers[idx].PlayerNumber, lockedIn ? NetworkCharSelection.SeatState.LockedIn : NetworkCharSelection.SeatState.Active, newSeatIdx);
        CloseLobbyIfReady();
    }
    int FindLobbyPlayerIdx(ulong clientId)
    {
        for (int i = 0; i < networkCharSelection.LobbyPlayers.Count; ++i)
        {
            if (networkCharSelection.LobbyPlayers[i].ClientId == clientId)
                return i;
        }
        return -1;
    }
    /// <summary>
    /// Checks foreach player in lobby if lockin choice, if not -> return
    /// else -> close lobby , save choice result and go to next scene
    /// </summary>
    void CloseLobbyIfReady()
    {
        
        foreach(NetworkCharSelection.PlayerStatus playerStatus in networkCharSelection.LobbyPlayers)
        {
            if(playerStatus.SeatState != NetworkCharSelection.SeatState.LockedIn)
            {
                return;
            }
        }
        networkCharSelection.IsLobbyClosed.Value = true;
        SaveLobbyResults();
        m_WaitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
    }

    private void SaveLobbyResults()
    {
        //TODO
        foreach(NetworkCharSelection.PlayerStatus playerStatus in networkCharSelection.LobbyPlayers)
        {
            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerStatus.ClientId);

            if(playerNetworkObject && playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer))
            {
                persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value = networkCharSelection.AvatarConfiguration[playerStatus.SeatIdx].Guid.ToNetworkGuid();
            }
        }
    }

    IEnumerator WaitToEndLobby()
    {
        yield return new WaitForSeconds(3);
        SceneLoaderWrapper.Instance.LoadScene("Game01", useNetworkSceneManager: true);
    }
}
