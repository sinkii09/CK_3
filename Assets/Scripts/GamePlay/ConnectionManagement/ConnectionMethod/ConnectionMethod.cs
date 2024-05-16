using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.BossRoom.Utils;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using static UnityServiceErrorMessage;

public abstract class ConnectionMethodBase
{
    protected ConnectionManager m_ConnectionManager;
    protected readonly string m_PlayerName;
    protected const string k_DtlsConnType = "dtls";
    protected const string k_ClientGUIDKey = "client_guid";
    public abstract Task SetupHostConnectionAsync();

    public abstract Task SetupClientConnectionAsync();

    public ConnectionMethodBase(ConnectionManager connectionManager, string playerName)
    {
        m_ConnectionManager = connectionManager;
        m_PlayerName = playerName;
    }
    protected void SetConnectionPayload(string playerId, string playerName)
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            playerId = playerId,
            playerName = playerName,
            isDebug = Debug.isDebugBuild
        });
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        m_ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
    }
    protected string GetPlayerId()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return ClientPrefs.GetGuid();
        }

        return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid();
    }
}
