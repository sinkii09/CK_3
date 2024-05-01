using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

[RequireComponent(typeof(NetworkAvatarGuidState))]
public class ClientAvatarGuidHandler : NetworkBehaviour
{
    [SerializeField]
    NetworkAvatarGuidState m_NetworkAvatarGuidState;

    [SerializeField]
    Animator m_GraphicsAnimator;

    public Animator graphicsAnimator => m_GraphicsAnimator;


    public event Action<GameObject> AvatarGraphicsSpawned;

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            InstantiateAvatar();
        }
    }

    private void InstantiateAvatar()
    {
        if(m_GraphicsAnimator.transform.childCount > 0)
        {
            return;
        }
        Instantiate(m_NetworkAvatarGuidState.RegisteredAvatar.Graphics, m_GraphicsAnimator.transform);
        m_GraphicsAnimator.Rebind();
        m_GraphicsAnimator.Update(0f);

        AvatarGraphicsSpawned?.Invoke(m_GraphicsAnimator.gameObject);
    }
}
