using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ServerAnimationHandler : MonoBehaviour
{
    [SerializeField]
    NetworkAnimator m_NetworkAnimator;

    [SerializeField]
    VisualizationConfiguration m_VisualizationConfiguration;

    public NetworkAnimator NetworkAnimator => m_NetworkAnimator;
}
