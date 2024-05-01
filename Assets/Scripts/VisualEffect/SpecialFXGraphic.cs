using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFXGraphic : MonoBehaviour
{
    [SerializeField]
    public List<ParticleSystem> m_ParticleSystemsToTurnOffOnShutdown;

    [SerializeField]
    private float m_AutoShutdownTime = -1;

    [SerializeField]
    private float m_PostShutdownSelfDestructTime = -1;

    bool m_StayAtSpawnRotation;

    private bool m_IsShutdown = false;

    private Coroutine coroWaitForSelfDestruct = null;

    Quaternion m_StartRotation;
    public void Shutdown()
    {

    }
}
