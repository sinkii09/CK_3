using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsProjectile : MonoBehaviour
{
    bool m_Started;

    ulong m_SpawnerId;

    ProjectileInfo m_ProjectileInfo;
    public void Initialize(ulong creatorsNetworkObjectId, in ProjectileInfo projectileInfo)
    {
        m_SpawnerId = creatorsNetworkObjectId;
        m_ProjectileInfo = projectileInfo;
    }
    private void FixedUpdate()
    {
        var displacement = transform.forward * (m_ProjectileInfo.Speed * Time.fixedDeltaTime);
        transform.position += displacement;
    }
}
