using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Avatar = Unity.CK.GamePlay.Configuration.Avatar;
using Unity.CK.GamePlay.Configuration;

public class NetworkAvatarGuidState : NetworkBehaviour
{
    [SerializeField]
    AvatarRegistry m_AvatarRegistry;

    Avatar m_Avatar;

    public Avatar RegisteredAvatar
    {
        get
        {
            if (m_Avatar == null)
            {
                RegisterAvatar();
            }
                return m_Avatar;
        }
    }

    public void SetRandomAvatar()
    {
        m_Avatar = m_AvatarRegistry.GetRandomAvatar();
    }

    void RegisterAvatar()
    {
        //if (m_Avatar != null)
        //{
        //    return;
        //}
        SetRandomAvatar();
        if (TryGetComponent<ServerCharacter>(out var serverCharacter))
        {
            serverCharacter.CharacterClass = m_Avatar.characterClass;
        }
    }
}
