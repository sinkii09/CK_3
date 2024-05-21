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
[Serializable]
public class Item : NetworkBehaviour, IPickuptable
{
    
    const float k_DespawnTime = 5;
    bool canPickup;
    public ItemConfig m_ItemConfig;

    public override void OnNetworkSpawn()
    {   
        if(!IsServer)
        {
            enabled = false;
            return;
        }
        canPickup = true;
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

        ApplyEffect();   
    }
    [Rpc(SendTo.Server,AllowTargetOverride = true)]
    public void DeSpawnObjectServerRpc(RpcParams target)
    {
        NetworkObject.Despawn();
    }
    public void DeSpawnObject()
    {
        DeSpawnObjectServerRpc(RpcTarget.Owner);
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
