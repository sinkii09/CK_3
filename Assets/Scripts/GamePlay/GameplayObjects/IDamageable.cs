using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void ReceiveHP(ServerCharacter inflicter, int HP);

    ulong NetworkObjectId { get; }

    Transform transform { get; }

    [Flags]
    public enum SpecialDamageFlags
    {
        None = 0,
        UnusedFlag = 1 << 0, // does nothing
        StunOnTrample = 1 << 1,
        NotDamagedByPlayers = 1 << 2,
    }
    SpecialDamageFlags GetSpecialDamageFlags();
    bool IsDamageable();
}
