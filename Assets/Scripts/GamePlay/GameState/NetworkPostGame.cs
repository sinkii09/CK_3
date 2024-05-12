using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class NetworkPostGame : NetworkBehaviour
{
    public NetworkVariable<WinState> WinState = new NetworkVariable<WinState>();

    [Inject]
    public void Construct(PersistentGameState persistentGameState)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            WinState.Value = persistentGameState.WinState;
        }
    }
}
