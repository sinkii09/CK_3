using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Avatar = Unity.CK.GamePlay.Configuration.Avatar;
using Unity.CK.GamePlay.Configuration;
using UnityEngine.Serialization;

public class NetworkAvatarGuidState : NetworkBehaviour
{
    [FormerlySerializedAs("AvatarGuidArray")]
    [HideInInspector]
    public NetworkVariable<NetworkGuid> AvatarGuid = new NetworkVariable<NetworkGuid>();

    [SerializeField]
    AvatarRegistry m_AvatarRegistry;

    Avatar m_Avatar;

    public Avatar RegisteredAvatar
    {
        get
        {
            if (m_Avatar == null)
            {
                RegisterAvatar(AvatarGuid.Value.ToGuid());
            }

            return m_Avatar;
        }
    }

    public void SetRandomAvatar()
    {
        AvatarGuid.Value = m_AvatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
    }

    void RegisterAvatar(Guid guid)
    {
        if (guid.Equals(Guid.Empty))
        {

            return;
        }
        // based on the Guid received, Avatar is fetched from AvatarRegistry
        //if (!m_AvatarRegistry.TryGetAvatar(guid, out var avatar))
        //{
        //    Debug.LogError("Avatar not found!");
        //    return;
        //}

        if (m_Avatar != null)
        {
            return;
        }

        m_Avatar = m_AvatarRegistry.GetRandomAvatar();

        if (TryGetComponent<ServerCharacter>(out var serverCharacter))
        {
            serverCharacter.CharacterClass = m_Avatar.characterClass;
        }
    }
}
