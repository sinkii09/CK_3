using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIStateDisplayHandler : NetworkBehaviour
{
    const float k_DurationSeconds = 2f;

    [SerializeField]
    bool m_DisplayHealth;

    [SerializeField]
    bool m_DisplayName;

    RectTransform m_UIStateRectTransform;

    [SerializeField]
    NetworkHealthState m_NetworkHealthState;

    ServerCharacter m_ServerCharacter;

    ClientAvatarGuidHandler m_ClientAvatarGuidHandler;

    NetworkAvatarGuidState m_NetworkAvatarGuidState;

    [SerializeField]
    int m_BaseHP;

    [SerializeField]
    Transform m_TransformToTrack;

    Transform m_CanvasTransform;

    [SerializeField]
    float m_VerticalWorldOffset;

    [SerializeField]
    float m_VerticalScreenOffset;

    Vector3 m_VerticalOffset;

    Vector3 m_WorldPos;

    [SerializeField]
    UIHealth m_UIHealth;

    private void Awake()
    {
        m_ServerCharacter = GetComponent<ServerCharacter>();
    }

    public override void OnNetworkSpawn()
    {
        if (!NetworkManager.Singleton.IsClient)
        {
            enabled = false;
            return;
        }

        if (m_DisplayHealth)
        {
            m_NetworkHealthState.HitPointsReplenished += DisplayUIHealth;
        }

        if (m_DisplayHealth)
        {
            DisplayUIHealth();
        }

    }
    void DisplayUIHealth()
    {
        if (m_NetworkHealthState == null)
        {
            return;
        }
        //TODO:

        m_UIHealth.Initialize(m_NetworkHealthState.HitPoints, m_BaseHP);
    }
    private void OnDisable()
    {
        if (!m_DisplayHealth)
        {
            return;
        }
        if (m_NetworkHealthState != null)
        {
            m_NetworkHealthState.HitPointsReplenished -= DisplayUIHealth;
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        
    }
}
