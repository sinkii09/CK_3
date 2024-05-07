using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerServerCharacter : NetworkBehaviour
{
    static List<ServerCharacter> s_ActivePlayers = new List<ServerCharacter>();

    [SerializeField]
    ServerCharacter m_CachedServerCharacter;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            s_ActivePlayers.Add(m_CachedServerCharacter);
        }
        else
        {
            enabled = false;
        }

    }

    void OnDisable()
    {
        s_ActivePlayers.Remove(m_CachedServerCharacter);
    }

    public override void OnNetworkDespawn()
    {
        //TODO:
        //if (IsServer)
        //{
        //    var movementTransform = m_CachedServerCharacter.Movement.transform;
        //    SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
        //    if (sessionPlayerData.HasValue)
        //    {
        //        var playerData = sessionPlayerData.Value;
        //        playerData.PlayerPosition = movementTransform.position;
        //        playerData.PlayerRotation = movementTransform.rotation;
        //        playerData.CurrentHitPoints = m_CachedServerCharacter.HitPoints;
        //        playerData.HasCharacterSpawned = true;
        //        SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
        //    }
        //}
    }
    public static List<ServerCharacter> GetPlayerServerCharacters()
    {
        return s_ActivePlayers;
    }
    public static ServerCharacter GetPlayerServerCharacter(ulong ownerClientId)
    {
        foreach (var playerServerCharacter in s_ActivePlayers)
        {
            if (playerServerCharacter.OwnerClientId == ownerClientId)
            {
                return playerServerCharacter;
            }
        }
        return null;
    }
}
