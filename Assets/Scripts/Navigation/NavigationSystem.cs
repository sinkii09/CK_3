using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    public const string NavigationSystemTag = "NavigationSystem";

    public event System.Action OnNavigationMeshChanged = delegate { };

    private bool m_NavMeshChanged;
    public void OnDynamicObstacleDisabled()
    {
        m_NavMeshChanged = true;
    }

    public void OnDynamicObstacleEnabled()
    {
        m_NavMeshChanged = true;
    }
    private void FixedUpdate()
    {
  
        if (m_NavMeshChanged)
        {
            OnNavigationMeshChanged.Invoke();
            m_NavMeshChanged = false;
        }
    }
}
