using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO:
public class OfflineState : ConnectionState
{
    const string k_MainMenuSceneName = "MainMenu";

    public override void Enter()
    {
        m_ConnectionManager.NetworkManager.Shutdown();
        if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
        {
            SceneLoaderWrapper.Instance.LoadScene(k_MainMenuSceneName, useNetworkSceneManager: false);
        }
    }

    public override void Exit()
    {

    }

    public override void StartClientIP(string playerName, string ipaddress, int port)
    {
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting);
    }

    public override void StartClientLobby(string playerName)
    {
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting);
    }

    public override void StartHostIP(string playerName, string ipaddress, int port)
    {
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost);
    }

    public override void StartHostLobby(string playerName)
    {
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost);
    }
}
