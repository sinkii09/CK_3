using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

class StartingHostState : OnlineState
{

    [Inject]
    LobbyServiceFacade m_LobbyServiceFacade;
    [Inject]
    LocalLobby m_LocalLobby;
    ConnectionMethodBase m_ConnectionMethod;

    public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
    {
        m_ConnectionMethod = baseConnectionMethod;
        return this;
    }

    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;

        if(clientId == m_ConnectionManager.NetworkManager.LocalClient.ClientId)
        {
            var payLoad = System.Text.Encoding.UTF8.GetString(connectionData);
            
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payLoad);
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
    }

    public override void Enter()
    {
        StartHost();   
    }

    public override void Exit()
    {
    }

    public override void OnServerStarted()
    {
        m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_HostState);
    }

    public override void OnServerStopped()
    {
        StartHostFailed();
    }

    async void StartHost()
    {
        try
        {
            await m_ConnectionMethod.SetupHostConnectionAsync();

            if(!m_ConnectionManager.NetworkManager.StartHost())
            {
                StartHostFailed();
            }
        }
        catch (System.Exception)
        {
            StartHostFailed();
            throw;
        }
    }
    void StartHostFailed()
    {
        m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }
}
