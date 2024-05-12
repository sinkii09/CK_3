using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.Configuration;
using Unity.Netcode;


public class NetworkCharSelection :NetworkBehaviour
{
    public enum SeatState : byte
    {
        Inactive,
        Active,
        LockedIn,
    }

    public struct PlayerStatus : IEquatable<PlayerStatus>, INetworkSerializable
    {
        public ulong ClientId;
        public int PlayerNumber;
        public int SeatIdx;
        public SeatState SeatState;

        public PlayerStatus(ulong clientId, int playerNumber, SeatState state, int seatIdx = -1)
        {
            ClientId = clientId;
            PlayerNumber = playerNumber;
            SeatIdx = seatIdx;
            SeatState = state;
        }

        public bool Equals(PlayerStatus other)
        {
            return ClientId == other.ClientId &&
                PlayerNumber == other.PlayerNumber &&
                SeatIdx == other.SeatIdx &&
                SeatState == other.SeatState;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerNumber);
            serializer.SerializeValue(ref SeatIdx);
            serializer.SerializeValue(ref SeatState);
        }
    }
    public Avatar[] AvatarConfiguration;

    public event System.Action<ulong, int, bool> OnClientChangedSeat;

    //TODO:
    private NetworkList<PlayerStatus> m_LobbyPlayers;
    public NetworkList<PlayerStatus> LobbyPlayers => m_LobbyPlayers;

    public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);
    private void Awake()
    {
        m_LobbyPlayers = new NetworkList<PlayerStatus>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeSeatServerRPC(ulong clientId, int seatIdx, bool lockedIn)
    {
        OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
    }
}
