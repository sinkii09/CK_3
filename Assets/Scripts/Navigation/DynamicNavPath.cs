using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DynamicNavPath : IDisposable
{
    const float k_RepathToleranceSqr = 9f;

    NavMeshAgent m_Agent;

    NavigationSystem m_NavigationSystem;

    Vector3 m_CurrentPathOriginalTarget;

    NavMeshPath m_NavMeshPath;

    List<Vector3> m_Path;

    Vector3 m_PositionTarget;

    Transform m_TransformTarget;

    public DynamicNavPath(NavMeshAgent agent, NavigationSystem navigationSystem)
    {
        m_Agent = agent;
        m_Path = new List<Vector3>();
        m_NavMeshPath = new NavMeshPath();
        m_NavigationSystem = navigationSystem;

        navigationSystem.OnNavigationMeshChanged += OnNavMeshChanged;
    }

    Vector3 TargetPosition => m_TransformTarget != null ? m_TransformTarget.position : m_PositionTarget;

    public void FollowTransform(Transform target)
    {
        m_TransformTarget = target;
    }

    public void SetTargetPosition(Vector3 target)
    {
        // If there is an nav mesh area close to the target use a point inside the nav mesh instead.
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            target = hit.position;
        }

        m_PositionTarget = target;
        m_TransformTarget = null;
        RecalculatePath();
    }

    void OnNavMeshChanged()
    {
        RecalculatePath();
    }
    public void Clear()
    {
        m_Path.Clear();
    }
    public Vector3 MoveAlongPath(float distance)
    {
        if (m_TransformTarget != null)
        {
            OnTargetPositionChanged(TargetPosition);
        }

        if (m_Path.Count == 0)
        {
            return Vector3.zero;
        }

        var currentPredictedPosition = m_Agent.transform.position;
        var remainingDistance = distance;

        while (remainingDistance > 0)
        {
            var toNextPathPoint = m_Path[0] - currentPredictedPosition;

            // If end point is closer then distance to move
            if (toNextPathPoint.sqrMagnitude < remainingDistance * remainingDistance)
            {
                currentPredictedPosition = m_Path[0];
                m_Path.RemoveAt(0);
                remainingDistance -= toNextPathPoint.magnitude;
            }

            // Move towards point
            currentPredictedPosition += toNextPathPoint.normalized * remainingDistance;

            // There is definitely no remaining distance to cover here.
            break;
        }

        return currentPredictedPosition - m_Agent.transform.position;
    }
    void OnTargetPositionChanged(Vector3 newTarget)
    {
        if (m_Path.Count == 0)
        {
            RecalculatePath();
        }

        if ((newTarget - m_CurrentPathOriginalTarget).sqrMagnitude > k_RepathToleranceSqr)
        {
            RecalculatePath();
        }
    }
    void RecalculatePath()
    {
        m_CurrentPathOriginalTarget = TargetPosition;
        m_Agent.CalculatePath(TargetPosition, m_NavMeshPath);

        m_Path.Clear();

        var corners = m_NavMeshPath.corners;

        for (int i = 1; i < corners.Length; i++)
        {
            m_Path.Add(corners[i]);
        }
    }
    public void Dispose()
    {
        m_NavigationSystem.OnNavigationMeshChanged -= OnNavMeshChanged;
    }
}
