using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class ConnectionButton : MonoBehaviour
{
    [Inject]
    ConnectionManager m_ConnectionManager;
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        m_ConnectionManager.StartHostIp(" "," ", 123);
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        m_ConnectionManager.StartClientIp(" ", " ", 123);
    }
}
