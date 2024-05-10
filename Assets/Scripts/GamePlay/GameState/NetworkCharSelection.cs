using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.Configuration;
using Unity.Netcode;


public class NetworkCharSelection :NetworkBehaviour
{
    public Avatar[] AvatarConfiguration;

    public event System.Action<ulong, int, bool> OnClientChangedSeat;

    //TODO:
    private NetworkList<ulong> m_LobbyPlayers;
    public NetworkList<ulong> LobbyPlayers => m_LobbyPlayers;

    private void Awake()
    {
        m_LobbyPlayers = new NetworkList<ulong>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSeatServerRPC(ulong clientId, int seatIdx, bool lockedIn)
    {
        OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
    }
}
