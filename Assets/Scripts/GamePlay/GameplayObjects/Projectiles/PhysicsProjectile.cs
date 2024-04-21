using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsProjectile : NetworkBehaviour
{
    bool m_Started;

    ulong m_SpawnerId;

    ProjectileInfo m_ProjectileInfo;

    float m_DestroyAtSec;

    public void Initialize(ulong creatorsNetworkObjectId, in ProjectileInfo projectileInfo)
    {
        m_SpawnerId = creatorsNetworkObjectId;
        m_ProjectileInfo = projectileInfo;
    }

    public override void OnNetworkSpawn()
    {
       if(IsServer)
       {
            m_Started = true;

            m_DestroyAtSec = Time.fixedTime + (m_ProjectileInfo.Range / m_ProjectileInfo.Speed);
       }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            m_Started = false;
        }
    }

    private void FixedUpdate()
    {
        if (!m_Started || !IsServer)
        {
            return;
        }
        if(m_DestroyAtSec < Time.fixedTime)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Despawn();
            return;
        }
        var displacement = transform.forward * (m_ProjectileInfo.Speed * Time.fixedDeltaTime);
        transform.position += displacement;
    }
}
