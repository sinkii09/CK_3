
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using VContainer;

class HostingState : OnlineState
{
    [Inject]
    LobbyServiceFacade m_LobbyServiceFacade;
    [Inject]
    IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

    const int k_MaxConnectPayload = 1024;
    public override void Enter()
    {
        SceneLoaderWrapper.Instance.LoadScene("CharacterSelect", true);

        if(m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.BeginTracking();
        }
    }
    public override void Exit()
    {
        SessionManager<SessionPlayerData>.Instance.OnServerEnded();
    }

    public override void OnClientConnected(ulong clientId)
    {
        var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
        if (playerData != null)
        {
            m_ConnectionEventPublisher.Publish(new ConnectionEventMessage()
            {
                ConnectStatus = ConnectStatus.Success,
                //TODO : playerName;
            }
            );
        }
        else
        {
            var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
            m_ConnectionManager.NetworkManager.DisconnectClient(clientId, reason); 
        }
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        if (clientId != m_ConnectionManager.NetworkManager.LocalClientId)
        {
            var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
            if(playerId != null)
            {
                var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                if (sessionData.HasValue)
                {
                    m_ConnectionEventPublisher.Publish(new ConnectionEventMessage()
                    { ConnectStatus = ConnectStatus.GenericDisconnect, });
                }
                SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
            }
        }
    }

    public override void OnUserRequestedShutdown()
    {
        var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
        for ( var  i = m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
        {
            var id = m_ConnectionManager.NetworkManager.ConnectedClientsIds[i];
            if(id != m_ConnectionManager.NetworkManager.LocalClientId)
            {
                m_ConnectionManager.NetworkManager.DisconnectClient(id,reason);
            }
        }
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;
        if(connectionData.Length > k_MaxConnectPayload)
        {
            response.Approved = false;
            return;
        }
        var payload = System.Text.Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
        var gameReturnStatus = GetConnectStatus(connectionPayload);
        Debug.Log(gameReturnStatus.ToString());
        if (gameReturnStatus == ConnectStatus.Success)
        {
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            return;
        }
        response.Approved = false;
        response.Reason = JsonUtility.ToJson(gameReturnStatus);
        if (m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.RemovePlayerFromLobbyAsync(connectionPayload.playerId);
        }
    }

    public override void OnServerStopped()
    {
        m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
    {
        if(m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= m_ConnectionManager.MaxConnectedPlayers)
        {
            return ConnectStatus.ServerFull;
        }
        if(connectionPayload.isDebug == Debug.isDebugBuild)
        {
            return ConnectStatus.IncompatibleBuildType;
        }
        return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
            ConnectStatus.LoggedInAgain : ConnectStatus.Success;
    }
}
