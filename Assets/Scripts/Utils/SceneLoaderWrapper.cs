using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderWrapper : NetworkBehaviour
{
    public static SceneLoaderWrapper Instance { get; protected set; }


    bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null && NetworkManager.NetworkConfig.EnableSceneManagement;

    bool m_IsInitialized;

    public virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public virtual void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkManager.OnServerStarted += OnNetworkingSessionStarted;
        NetworkManager.OnClientStarted += OnNetworkingSessionStarted;
        NetworkManager.OnServerStopped += OnNetworkingSessionEnded;
        NetworkManager.OnClientStopped += OnNetworkingSessionEnded;
    }
    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if(NetworkManager!=null)
        {
            NetworkManager.OnServerStarted -= OnNetworkingSessionStarted;
            NetworkManager.OnClientStarted -= OnNetworkingSessionStarted;
            NetworkManager.OnServerStopped -= OnNetworkingSessionEnded;
            NetworkManager.OnClientStopped -= OnNetworkingSessionEnded;
        }
        base.OnDestroy();
    }


    private void OnNetworkingSessionStarted()
    {
        if(!m_IsInitialized)
        {
            if(IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent += OnsceneEvent;
            }

            m_IsInitialized = true;
        }
    }

    private void OnNetworkingSessionEnded(bool obj)
    {
        if (m_IsInitialized)
        {
            if (IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnsceneEvent;
            }

            m_IsInitialized = false ;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!IsSpawned || NetworkManager.ShutdownInProgress)
        {
            //TODO
            //m_ClientLoadingScreen.StopLoadingScreen();
        }
    }
    void OnsceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                if (NetworkManager.IsClient)
                {
                    //TODO
                    //if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                    //{
                    //    m_ClientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                    //    m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                    //}
                    //else
                    //{
                    //    m_ClientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);
                    //    m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                    //}
                }
                break;
            case SceneEventType.LoadEventCompleted:
                if (NetworkManager.IsClient)
                {
                   // TODO
                   // m_ClientLoadingScreen.StopLoadingScreen();
                }
                break;
            case SceneEventType.Synchronize: 
                if(NetworkManager.IsClient && !NetworkManager.IsHost)
                {
                    if(NetworkManager.SceneManager.ClientSynchronizationMode == LoadSceneMode.Single)
                    {
                        UnloadAdditiveScenes();
                    }
                }
                break;
            case SceneEventType.SynchronizeComplete: 
                if(NetworkManager.IsServer)
                {
                    StopLoadingScreenClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } } });
                }
                break;
        }
    }

    public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if(useNetworkSceneManager)
        {
            if(IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
            {
                if(NetworkManager.IsServer)
                {
                    NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                }
            }
        }
        else
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (loadSceneMode == LoadSceneMode.Single)
            {
                //TODO:
                //m_ClientLoadingScreen.StartLoadingScreen(sceneName);
                //m_LoadingProgressManager.LocalLoadOperation = loadOperation;
            }
        }
    }

    void UnloadAdditiveScenes()
    {
        var activeScene = SceneManager.GetActiveScene();
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene != activeScene)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
    [ClientRpc]
    void StopLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
    {
        //TODO
     //   m_ClientLoadingScreen.StopLoadingScreen();
    }
}
