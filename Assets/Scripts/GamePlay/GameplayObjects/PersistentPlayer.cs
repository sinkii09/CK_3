using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PersistentPlayer : NetworkBehaviour
{

    [SerializeField]
    NetworkAvatarGuidState m_NetworkAvatarGuidState;

    public NetworkAvatarGuidState NetworkAvatarGuidState => m_NetworkAvatarGuidState;

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnNetworkSpawn()
    {
        gameObject.name = "PersistentPlayer" + OwnerClientId;
        if (IsServer)
        {
            m_NetworkAvatarGuidState.SetRandomAvatar();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
}
