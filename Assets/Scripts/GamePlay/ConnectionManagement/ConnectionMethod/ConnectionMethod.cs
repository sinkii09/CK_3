using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ConnectionMethodBase
{
    protected ConnectionManager m_ConnectionManager;
    protected readonly string m_PlayerName;
    protected const string k_DtlsConnType = "dtls";

    public abstract Task SetupHostConnectionAsync();

    public abstract Task SetupClientConnectionAsync();

    public ConnectionMethodBase(ConnectionManager connectionManager, string playerName)
    {
        m_ConnectionManager = connectionManager;
        m_PlayerName = playerName;
    }
}
