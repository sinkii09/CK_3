using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
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
    PersistentGameState persistentGameState;

    [SerializeField]
    NetcodeHooks m_NetcodeHooks;

    [SerializeField]
    Countdown m_Countdown;

    [SerializeField]
    private NetworkObject m_PlayerPrefab;

    [SerializeField]
    private Transform[] m_PlayerSpawnPoints;

    private List<Transform> m_PlayerSpawnPointsList = null;
    
    public bool InitialSpawnDone { get; private set; }

    [Inject] ISubscriber<LifeStateChangedEventMessage> m_LifeStateChangedEventMessageSubscriber;

    [Inject] ConnectionManager m_ConnectionManager;

    [Inject] PersistentGameState m_PersistentGameState;
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
        m_PersistentGameState.Reset();
        //m_LifeStateChangedEventMessageSubscriber.Subscribe(OnLifeStateChangedEventMessage);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventComplete;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;

        m_Countdown.PlayTimeExpired += CheckForGameOver;
    }

    void OnNetworkDespawn()
    {
        //if (m_LifeStateChangedEventMessageSubscriber != null)
        //{
        //    m_LifeStateChangedEventMessageSubscriber.Unsubscribe(OnLifeStateChangedEventMessage);
        //}
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventComplete;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;

        m_Countdown.PlayTimeExpired -= CheckForGameOver;
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
        m_Countdown.StartCountdown();
    }

    private void SpawnPlayer(ulong clientId, bool lateJoin)
    {
        //TODO

        Transform spawnPoint = null;

        if(m_PlayerSpawnPointsList == null || m_PlayerSpawnPointsList.Count == 0)
        {
            m_PlayerSpawnPointsList = new List<Transform>(m_PlayerSpawnPoints);
        }

        int index = UnityEngine.Random.Range(0, m_PlayerSpawnPointsList.Count);
        spawnPoint = m_PlayerSpawnPointsList[index];
        m_PlayerSpawnPointsList.RemoveAt(index);
        var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

        var newPlayer = Instantiate(m_PlayerPrefab , Vector3.zero , Quaternion.identity);
        
        var newPlayerCharacter = newPlayer.GetComponent<ServerCharacter>();

        var physicsTransform = newPlayerCharacter.physicsWrapper.Transform;

        if (spawnPoint != null)
        {
            physicsTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }
        var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
        
        Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");
        
        var networkAvatarGuidStateExists = newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);
        
        Assert.IsTrue(networkAvatarGuidStateExists,
                $"NetworkCharacterGuidState not found on player avatar!");
        
        networkAvatarGuidState.AvatarGuid = new NetworkVariable<NetworkGuid>(persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value);
        newPlayer.SpawnWithOwnership(clientId, true);
    }
    private void OnClientDisconnectCallback(ulong clientId)
    {
        if(clientId != NetworkManager.Singleton.LocalClientId)
        {
            StartCoroutine(WaitToCheckForGameOver());
        }
    }
    IEnumerator WaitToCheckForGameOver()
    {
        yield return null;
        //CheckForGameOver();
    }

    #region FIX GamePlay
    //TODO
    //private void OnLifeStateChangedEventMessage(LifeStateChangedEventMessage message)
    //{
    //    switch(message.CharacterType)
    //    {
    //        case CharacterTypeEnum.Warrior:
    //        case CharacterTypeEnum.Archer:
    //        case CharacterTypeEnum.Mage:
    //        case CharacterTypeEnum.Rogue:
    //            if(message.LifeState == LifeState.Fainted)
    //            {
    //                CheckForGameOver();
    //            }
    //            break;
    //        case CharacterTypeEnum.Boss:
    //            {
    //                if(message.LifeState == LifeState.Dead)
    //                {
    //                    BossDefeated();
    //                }
    //                break;
    //            }
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }
    //}
    //private void CheckForGameOver()
    //{
    //    foreach (var serverCharacter in PlayerServerCharacter.GetPlayerServerCharacters())
    //    {
    //        if(serverCharacter != null && serverCharacter.LifeState == LifeState.Alive)
    //        {
    //            return;
    //        }
    //    }
    //    StartCoroutine(CoroGameOver(2,false)) ;
    //}
    //void BossDefeated()
    //{
    //    StartCoroutine(CoroGameOver(5,true));
    //}
    private void CheckForGameOver()
    {
        //StartCoroutine(CoroGameOver(2));
    }
    IEnumerator CoroGameOver(float delay)
    { 
        yield return new WaitForSeconds(delay);

        SceneLoaderWrapper.Instance.LoadScene("PostGame",useNetworkSceneManager:true);
    }
    #endregion
}
