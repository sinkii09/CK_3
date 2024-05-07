using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RotationLerper 
{
    Quaternion m_LerpStart;

    // Calculated time elapsed for the most recent interpolation
    float m_CurrentLerpTime;

    // The duration of the interpolation, in seconds
    float m_LerpTime;

    public RotationLerper(Quaternion start, float lerpTime)
    {
        m_LerpStart = start;
        m_CurrentLerpTime = 0f;
        m_LerpTime = lerpTime;
    }
    public Quaternion LerpRotation(Quaternion current, Quaternion target)
    {
        if (current != target)
        {
            m_LerpStart = current;
            m_CurrentLerpTime = 0f;
        }

        m_CurrentLerpTime += Time.deltaTime;
        if (m_CurrentLerpTime > m_LerpTime)
        {
            m_CurrentLerpTime = m_LerpTime;
        }

        var lerpPercentage = m_CurrentLerpTime / m_LerpTime;

        return Quaternion.Slerp(m_LerpStart, target, lerpPercentage);
    }
}
