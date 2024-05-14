using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public enum ConnectStatus
{
    Undefined,
    Success,
    ServerFull,
    UserRequestedDisconnect,
    GenericDisconnect,
    IncompatibleBuildType,
    HostEndedSession,
    StartHostFailed,
    StartClientFailed,
    LoggedInAgain,
    Reconnecting,
}
public struct ConnectionEventMessage : INetworkSerializeByMemcpy
{
    public ConnectStatus ConnectStatus;
}
public class ConnectionManager : MonoBehaviour
{
    [Inject]
    IObjectResolver m_Resolver;

    [Inject]
    NetworkManager m_networkManager;
    public NetworkManager NetworkManager => m_networkManager;

    ConnectionState m_CurrentState;

    public int MaxConnectedPlayers = 4;

    internal readonly OfflineState m_Offline = new OfflineState();
    internal readonly StartingHostState m_StartingHost = new StartingHostState();
    internal readonly HostingState m_HostState = new HostingState();
    internal readonly ClientConnectedState m_ClientConnected = new ClientConnectedState();
    internal readonly ClientConnectingState m_ClientConnecting = new ClientConnectingState();
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        List<ConnectionState> states = new() { m_ClientConnected, m_ClientConnecting, m_HostState, m_StartingHost, m_Offline };
        foreach (ConnectionState state in states)
        {
            m_Resolver.Inject(state);
        }
        m_CurrentState = m_Offline;

        NetworkManager.OnServerStarted += OnServerStarted;
        NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
        NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.OnTransportFailure += OnTransportFailure;
        NetworkManager.OnServerStopped += OnServerStopped;
    }

    

    private void OnDestroy()
    {
        NetworkManager.OnServerStarted -= OnServerStarted;
        NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.OnTransportFailure -= OnTransportFailure;
        NetworkManager.OnServerStopped -= OnServerStopped;
    }
    internal void ChangeState(ConnectionState nextState)
    {
        if(m_CurrentState != null)
        {
            m_CurrentState.Exit();
        }
        m_CurrentState = nextState;
        m_CurrentState.Enter();
    }
    private void OnServerStarted()
    {
        m_CurrentState.OnServerStarted();
    }
    private void OnServerStopped(bool obj)
    {
        m_CurrentState.OnServerStopped();
    }

    private void OnTransportFailure()
    {
        m_CurrentState?.OnTransportFailure();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        m_CurrentState.ApprovalCheck(request, response);
    }

    private void OnClientDisconnectCallback(ulong ClientId)
    {
        m_CurrentState.OnClientDisconnect(ClientId);
    }

    private void OnClientConnectedCallback(ulong ClientId)
    {
        m_CurrentState.OnClientConnected(ClientId);
    }
    public void StartClientLobby(string playerName)
    {
        m_CurrentState.StartClientLobby(playerName);
    }

    public void StartClientIp(string playerName, string ipaddress, int port)
    {
        m_CurrentState.StartClientIP(playerName, ipaddress, port);
    }

    public void StartHostLobby(string playerName)
    {
        m_CurrentState.StartHostLobby(playerName);
    }

    public void StartHostIp(string playerName, string ipaddress, int port)
    {
        m_CurrentState.StartHostIP(playerName, ipaddress, port);
    }

    public void RequestShutdown()
    {
        m_CurrentState.OnUserRequestedShutdown();
    }
}
