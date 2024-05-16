using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

class ClientConnectingState : OnlineState
{
    protected ConnectionMethodBase m_ConnectionMethod;

    public ClientConnectingState Configure(ConnectionMethodBase connectionMethod)
    {
        m_ConnectionMethod = connectionMethod;
        return this;
    }

    public override void Enter()
    {
#pragma warning disable 4014
        ConnectClientAsync();
#pragma warning restore 4014
    }

    public override void Exit()
    {
    }

    public override void OnClientConnected(ulong clientId)
    {
        Debug.Log("change to client connected");
        m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        StartingClientFailed();
    }

    private void StartingClientFailed()
    {
        var disconnectReason = m_ConnectionManager.NetworkManager.DisconnectReason;
        Debug.Log(disconnectReason.ToString());
        if(string.IsNullOrEmpty(disconnectReason))
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
        }
        else
        {
            var connectionStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
            m_ConnectStatusPublisher.Publish(connectionStatus);
        }
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    internal async Task ConnectClientAsync()
    {
        try
        {
            await m_ConnectionMethod.SetupClientConnectionAsync();
            if(!m_ConnectionManager.NetworkManager.StartClient())
            {
                throw new Exception("NetworkManager StartClient failed");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            StartingClientFailed();
            throw;
        }
    }
}
