using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharSelection :NetworkBehaviour
{
    public Avatar[] AvatarConfiguration;

    public event System.Action<ulong, int, bool> OnClientChangedSeat;

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSeatServerRPC(ulong clientId, int seatIdx, bool lockedIn)
    {
        OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
    }
}
