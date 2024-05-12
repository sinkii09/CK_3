using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

[RequireComponent(typeof(NetcodeHooks))]
public class ServerPostGameState : GameStateBehaviour
{
    public override GameState ActiveState { get { return GameState.PostGame; } }

    const string k_CharSelectSceneName = "CharacterSelect";

    const string k_MainMenuSceneName = "MainMenu";


    [SerializeField]
    NetcodeHooks m_NetcodeHooks;
    [SerializeField]
    NetworkPostGame networkPostGame;
    public NetworkPostGame NetworkPostGame => networkPostGame;



    [Inject]
    ConnectionManager m_ConnectionManager;
    [Inject]
    PersistentGameState m_PersistentGameState;
    protected override void Awake()
    {
        base.Awake();
        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
    }

    protected override void OnDestroy()
    {
        m_PersistentGameState.Reset();
        ActionFactory.PurgePooledActions();
        base.OnDestroy();
        m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
    }

    private void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsServer) { enabled = false; }
        else
        {
            //TODO:
            //SessionManager<SessionPlayerData>.Instance.OnSessionEnded();
            networkPostGame.WinState.Value = m_PersistentGameState.WinState;
        }
    }
    public void PlayAgain()
    {
        SceneLoaderWrapper.Instance.LoadScene(k_CharSelectSceneName,useNetworkSceneManager:true);
    }

    public void GoToMainMenu()
    {
        //TODO: request shutdown
        m_ConnectionManager.NetworkManager.Shutdown();

        if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
        {
            SceneLoaderWrapper.Instance.LoadScene(k_MainMenuSceneName, useNetworkSceneManager: false);
        }
    }

}
