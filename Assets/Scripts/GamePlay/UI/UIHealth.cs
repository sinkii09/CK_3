using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField] Slider m_HitPointsSlider;

    NetworkVariable<int> m_NetworkedHealth;

    public void Initialize(NetworkVariable<int> networkedHealth, int maxValue)
    {
        m_NetworkedHealth = networkedHealth;

        m_HitPointsSlider.minValue = 0;
        m_HitPointsSlider.maxValue = maxValue;

        HealthChanged(maxValue, maxValue);

        m_NetworkedHealth.OnValueChanged += HealthChanged;
    }

    void HealthChanged(int previousValue, int newValue)
    {
        m_HitPointsSlider.value = newValue;
    }

    private void OnDestroy()
    {
        m_NetworkedHealth.OnValueChanged -= HealthChanged;
    }
}
