using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.Configuration;
using UnityEngine;

namespace Unity.CK.GamePlay.Configuration
{
    [CreateAssetMenu]
    public class AvatarRegistry : ScriptableObject
    {
        [SerializeField]
        Avatar[] m_Avatars;

        public Avatar GetRandomAvatar()
        {
            if (m_Avatars == null || m_Avatars.Length == 0)
            {
                return null;
            }
            return m_Avatars[Random.Range(0, m_Avatars.Length)];
        }

    }
}


