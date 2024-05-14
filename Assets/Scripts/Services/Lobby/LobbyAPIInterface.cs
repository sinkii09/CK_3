using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyAPIInterface 
{
    const int k_MaxLobbiesToShow = 16;

    readonly List<QueryFilter> m_Filters;
    readonly List<QueryOrder> m_Order;

    public LobbyAPIInterface()
    {
        m_Filters = new List<QueryFilter>()
        {
            new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0",QueryFilter.OpOptions.GT)
        };
        m_Order = new List<QueryOrder>()
        {
            new QueryOrder(false,QueryOrder.FieldOptions.Created)
        };
    }
    public async Task<Lobby> CreateLobby(string requesterUserId, string lobbyName,int maxPlayers, bool isPrivate, Dictionary<string,PlayerDataObject> hostUserData, Dictionary<string,DataObject> lobbyData)
    {
        CreateLobbyOptions options = new CreateLobbyOptions()
        {
            IsPrivate = isPrivate,
            IsLocked = true,
            Player = new Player(id: requesterUserId, data: hostUserData),
            Data = lobbyData
        };
        return await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
    }
    public async Task DeleteLobby(string lobbyId)
    {
        await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
    }

    public async Task<Lobby> JoinLobbyByCode(string requesterUserId, string lobbyCode, Dictionary<string, PlayerDataObject> localUserData)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Player = new Player(id: requesterUserId, data: localUserData)
        };
        return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
    }
    public async Task<Lobby> JoinLobbyById(string requesterUserId, string lobbyId, Dictionary<string, PlayerDataObject> localUserData)
    {
        JoinLobbyByIdOptions options = new JoinLobbyByIdOptions 
        {
            Player = new Player(id: requesterUserId, data: localUserData) 
        };
        return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
    }
    public async Task<Lobby> QuickJoinLobby(string requesterUserId, Dictionary<string, PlayerDataObject> localUserData)
    {
        QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
        {
            Filter = m_Filters,
            Player = new Player(id: requesterUserId, data: localUserData)
        };
        return await LobbyService.Instance.QuickJoinLobbyAsync(options);
    }
    public async Task<Lobby> ReconnectToLobby(string lobbyId)
    {
        return await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);
    }
    public async Task RemovePlayerFromLobby(string requesterUserId, string lobbyId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, requesterUserId);
        }
        catch (LobbyServiceException e) when (e is { Reason: LobbyExceptionReason.PlayerNotFound })
        {
            
        }
    }
    public async Task<QueryResponse> QueryAllLobbies()
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions
        {
            Count = k_MaxLobbiesToShow,
            Filters = m_Filters,
            Order = m_Order
        };
        return await LobbyService.Instance.QueryLobbiesAsync(options);
    }
    public async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions
        {
            Data = data,
            IsLocked = shouldLock
        };
        return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
    }
    public async Task<Lobby> UpdatePlayer(string lobbyId, string playerId, Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = data,
            AllocationId = allocationId,
            ConnectionInfo = connectionInfo
        };
        return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, options);
    }
    public async void SendHeartbeatPing(string lobbyId)
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
    }
    public async Task<ILobbyEvents> SubscribeToLobby(string lobbyId, LobbyEventCallbacks eventCallbacks)
    {
        return await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, eventCallbacks);
    }

}
