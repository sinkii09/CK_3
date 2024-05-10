using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

[RequireComponent(typeof(NetcodeHooks))]
public class ServerGamePlayState : GameStateBehaviour
{
    public override GameState ActiveState { get { return GameState.Game01; } }

    [SerializeField]
    NetcodeHooks m_NetcodeHooks;

    [SerializeField]
    private NetworkObject m_PlayerPrefab;

    public bool InitialSpawnDone { get; private set; }


    [Inject] ConnectionManager m_ConnectionManager;

    protected override void Awake()
    {
        base.Awake();
        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
    }

    protected override void OnDestroy()
    {
        if (m_NetcodeHooks)
        {
            m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
        }
        base.OnDestroy();
    }

    void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            enabled = false;
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventComplete;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
    }

    void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventComplete;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
    }

    private void OnSynchronizeComplete(ulong clientId)
    {
        if(InitialSpawnDone && !PlayerServerCharacter.GetPlayerServerCharacter(clientId))
        {
            Debug.Log(clientId + "LateJoin");
            SpawnPlayer(clientId,true); 
        }
    }

    private void OnLoadEventComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
        {
            InitialSpawnDone = true;
            foreach(var client in NetworkManager.Singleton.ConnectedClients)
            {
                SpawnPlayer(client.Key, false);
            }
        }
    }

    private void SpawnPlayer(ulong clientId, bool lateJoin)
    {
        //TODO
        var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

        var newPlayer = Instantiate(m_PlayerPrefab , Vector3.zero , Quaternion.identity);

        var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
        
        Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");
        
        var networkAvatarGuidStateExists = newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);
        
        Assert.IsTrue(networkAvatarGuidStateExists,
                $"NetworkCharacterGuidState not found on player avatar!");
        
        networkAvatarGuidState.AvatarGuid = new NetworkVariable<NetworkGuid>(persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value);
        newPlayer.SpawnWithOwnership(clientId, true);
    }
}
