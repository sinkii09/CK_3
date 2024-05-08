using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AoeActionInput : BaseActionInput
{
    [SerializeField]
    GameObject m_InRangeVisualization;

    [SerializeField]
    GameObject m_OutOfRangeVisualization;

    bool m_ReceivedMouseDownEvent;

    NavMeshHit m_NavMeshHit;

    static readonly Plane k_Plane = new Plane(Vector3.up, 0f);

    private void Start()
    {
        var radius = GameDataSource.Instance.GetActionPrototypeByID(m_ActionID).Config.Radius;
    
        transform.localScale = new Vector3(radius, radius, radius) * 2;
    }
    private void Update()
    {
        if(PlaneRaycast(k_Plane,Camera.main.ScreenPointToRay(Input.mousePosition), out Vector3 pointOnPlane)&& NavMesh.SamplePosition(pointOnPlane, out m_NavMeshHit, 2f, NavMesh.AllAreas))
        {
            transform.position = m_NavMeshHit.position;
        }
        float range = GameDataSource.Instance.GetActionPrototypeByID(m_ActionID).Config.Range;
        bool isInRange = (m_Origin - transform.position).sqrMagnitude <= range*range;
        m_InRangeVisualization.SetActive(isInRange);
        m_OutOfRangeVisualization.SetActive(!isInRange);

        if(Input.GetMouseButtonDown(0))
        {
            m_ReceivedMouseDownEvent = true;
        }
        if(Input.GetMouseButtonUp(0) && m_ReceivedMouseDownEvent)
        {
            if(isInRange)
            {
                var data = new ActionRequestData
                {
                    Position = transform.position,
                    Direction = transform.position,
                    ActionID = m_ActionID,
                    ShouldQueue = false,
                    CancelMovement = true,
                    TargetIds = null
                };
                m_SendInput(data);
            }
            Destroy(gameObject);
            return;
        }
        else if(Input.GetMouseButtonDown(1))
        {
            Destroy(gameObject);
        }
    }
    static bool PlaneRaycast(Plane plane, Ray ray, out Vector3 pointOnPlane)
    {
        if(plane.Raycast(ray, out var enter))
        {
            pointOnPlane = ray.GetPoint(enter);
            return true;
        }
        else
        {
            pointOnPlane= Vector3.zero;
            return false;
        }
    }
}
