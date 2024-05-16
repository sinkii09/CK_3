using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

class ClientConnectedState : OnlineState
{
    [Inject]
    protected LobbyServiceFacade m_LobbyServiceFacade;

    public override void Enter()
    {
        Debug.Log("ClientConnected");
        //SceneLoaderWrapper.Instance.LoadScene("CharacterSelect", false);
        if (m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.BeginTracking();
        }
    }

    public override void Exit()
    {
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        var disconnectReason = m_ConnectionManager.NetworkManager.DisconnectReason;
        //TODO: reconect state
        var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
        m_ConnectStatusPublisher.Publish(connectStatus);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }
}
