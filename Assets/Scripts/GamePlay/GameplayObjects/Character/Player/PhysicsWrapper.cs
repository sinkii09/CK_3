using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsWrapper : NetworkBehaviour
{
    static Dictionary<ulong, PhysicsWrapper> m_PhysicsWrappers = new Dictionary<ulong, PhysicsWrapper>();

    [SerializeField]
    Transform m_Transform;

    [SerializeField]
    Collider m_DamageCollider;
    public Transform Transform => m_Transform;
    public Collider DamageCollider => m_DamageCollider;

    ulong m_NetworkObjectID;

    public override void OnNetworkSpawn()
    {
        m_PhysicsWrappers.Add(NetworkObjectId, this);

        m_NetworkObjectID = NetworkObjectId;
    }
    public override void OnNetworkDespawn()
    {
        RemovePhysicsWrapper();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        RemovePhysicsWrapper();
    }
    void RemovePhysicsWrapper()
    {
        m_PhysicsWrappers.Remove(m_NetworkObjectID);
    }
    public static bool TryGetPhysicsWrapper(ulong networkObjectID, out PhysicsWrapper physicsWrapper)
    {
        return m_PhysicsWrappers.TryGetValue(networkObjectID, out physicsWrapper);
    }
}
