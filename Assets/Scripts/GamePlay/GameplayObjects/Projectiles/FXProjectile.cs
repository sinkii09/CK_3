using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXProjectile : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> m_ProjectileGraphics;

    [SerializeField]
    private List<GameObject> m_TargetHitGraphics;

    [SerializeField]
    private List<GameObject> m_TargetMissedGraphics;

    [SerializeField]
    private float m_PostImpactDurationSeconds = 1;

    private Vector3 m_StartPoint;
    private Transform m_TargetDestination;
    private Vector3 m_MissDestination;
    private float m_FlightDuration;
    private float m_Age;
    private bool m_HasImpacted;

    public void Initialize(Vector3 startPoint, Transform target, Vector3 missPos, float flightTime)
    {
        m_StartPoint = startPoint;
        m_TargetDestination = target;
        m_MissDestination = missPos;
        m_FlightDuration = flightTime;
        m_HasImpacted = false;

        foreach (var projectileGO in m_ProjectileGraphics)
        {
            projectileGO.SetActive(true);
        }
    }
    public void Cancel()
    {
        Destroy(gameObject);
    }
    private void Update()
    {
        m_Age += Time.deltaTime;
        if (!m_HasImpacted)
        {
            if (m_Age >= m_FlightDuration)
            {
                Impact();
            }
            else
            {
               
                float progress = m_Age / m_FlightDuration;
                transform.position = Vector3.Lerp(m_StartPoint, m_TargetDestination ? m_TargetDestination.position : m_MissDestination, progress);
            }
        }
        else if (m_Age >= m_FlightDuration + m_PostImpactDurationSeconds)
        {
            Destroy(gameObject);
        }
    }
    private void Impact()
    {
        m_HasImpacted = true;

        foreach (var projectileGO in m_ProjectileGraphics)
        {
            projectileGO.SetActive(false);
        }
        if (m_TargetDestination)
        {
            foreach (var hitGraphicGO in m_TargetHitGraphics)
            {
                hitGraphicGO.SetActive(true);
            }
        }
        else
        {
            foreach (var missGraphicGO in m_TargetMissedGraphics)
            {
                missGraphicGO.SetActive(true);
            }
        }
    }
}
