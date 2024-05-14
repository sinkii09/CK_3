using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class ConnectionMethodRelay : ConnectionMethodBase
{
    LobbyServiceFacade m_LobbyServiceFacade;
    LocalLobby m_LocalLobby;

    public ConnectionMethodRelay(LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby,ConnectionManager connectionManager, string playerName) : base(connectionManager, playerName)
    {
        m_ConnectionManager = connectionManager;
        m_LocalLobby = localLobby;
        m_LobbyServiceFacade = lobbyServiceFacade;
    }

    public override async Task SetupClientConnectionAsync()
    {
        if(m_LobbyServiceFacade.CurrentUnityLobby ==  null)
        {
            throw new Exception("Trying to start relay while Lobby isn't set");
        }

        var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(m_LocalLobby.RelayJoinCode);

        await m_LobbyServiceFacade.UpdatePlayerDataAsync(joinedAllocation.AllocationId.ToString(), m_LocalLobby.RelayJoinCode);

        var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetRelayServerData(new RelayServerData(joinedAllocation, k_DtlsConnType));
    }

    public override async Task SetupHostConnectionAsync()
    {
        Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(m_ConnectionManager.MaxConnectedPlayers);

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
    
        m_LocalLobby.RelayJoinCode = joinCode;

        await m_LobbyServiceFacade.UpdateLobbyDataAndUnlockAsync();
        await m_LobbyServiceFacade.UpdatePlayerDataAsync(hostAllocation.AllocationIdBytes.ToString(),joinCode);

        var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
        utp.SetRelayServerData(new RelayServerData(hostAllocation, k_DtlsConnType));
    }
}
