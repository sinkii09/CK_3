using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class ConnectionManager : MonoBehaviour
{
    [Inject]
    NetworkManager m_networkManager;
    public NetworkManager NetworkManager => m_networkManager;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        NetworkManager.OnServerStarted += OnServerStarted;
    }
    private void OnDestroy()
    {
        NetworkManager.OnServerStarted -= OnServerStarted;
    }
    private void OnServerStarted()
    {
        SceneLoaderWrapper.Instance.LoadScene("CharacterSelect", useNetworkSceneManager:true);
    }
}
