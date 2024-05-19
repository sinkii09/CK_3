using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ItemType
{
    damageItem,
    buffItem,
    debuffItem,
}
public class Item : NetworkBehaviour, IPickuptable
{
    const float k_DespawnTime = 5;
    bool canPickup = true;
    public ItemConfig m_ItemConfig;

    public override void OnNetworkSpawn()
    {
        StartCoroutine(DeSpawnOverTime());
    }
    IEnumerator DeSpawnOverTime()
    {
        yield return new WaitForSeconds(k_DespawnTime);
        if (NetworkObject != null)
        {
            NetworkObject.Despawn(true);
        }
    }
    public void Pickup(ServerCharacter serverCharacter)
    {
        serverCharacter.HeldItem.Value = NetworkObjectId;
        if (NetworkObject != null)
        {
            NetworkObject.Despawn(true);
        }
        ApplyEffect();   
    }

    private void ApplyEffect()
    {
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ServerCharacter serverCharacter))
        {
            if(canPickup)
            {
                canPickup = false;
                Pickup(serverCharacter);
            }
        }
    }

}
