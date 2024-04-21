using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ClientInputSender : NetworkBehaviour
{
    #region Const

    const float k_MouseInputRaycastDistance = 100f;

   
    const float k_MoveSendRateSeconds = 0.04f; //25 fps.

    const float k_TargetMoveTimeout = 0.45f;

    const float k_MaxNavMeshDistance = 1f;
    #endregion

    #region Variables

    float m_LastSentMove;

    // Cache raycast hit array so that we can use non alloc raycasts
    readonly RaycastHit[] k_CachedHit = new RaycastHit[4];


    LayerMask m_GroundLayerMask;
    LayerMask m_ActionLayerMask;

    RaycastHitComparer m_RaycastHitComparer;

    bool m_MoveRequest;
    #endregion

    #region Events

    public event Action<Vector3> ClientMoveEvent;

    #endregion

    #region Ref
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField]
    InputManager m_InputManager;
    #endregion
    public override void OnNetworkSpawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
        m_InputManager.FireEvent += HandleFire;

        m_GroundLayerMask = LayerMask.GetMask(new[] { "Ground" });
        m_ActionLayerMask = LayerMask.GetMask(new[] { "Player"," NPCs" , "Ground" });
    }
    public override void OnNetworkDespawn()
    {
        if(m_ServerCharacter)
        {
            m_InputManager.FireEvent -= HandleFire;
        }
    }
    private void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if(m_InputManager.IsRightMouseButtonDownThisFrame())
            {
                m_MoveRequest = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if(EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }
        if (m_MoveRequest)
        {
            m_MoveRequest = false;
            if((Time.time - m_LastSentMove) > k_MoveSendRateSeconds)
            {
                m_LastSentMove = Time.time;
                Ray ray = Camera.main.ScreenPointToRay(m_InputManager.GetMouseScreenPosition());

                var groundHits = Physics.RaycastNonAlloc(ray, k_CachedHit, k_MouseInputRaycastDistance, m_GroundLayerMask);
                if(groundHits > 0)
                {
                    if(groundHits > 1)
                    {
                        Array.Sort(k_CachedHit,0, groundHits,m_RaycastHitComparer);
                    }
                    if (NavMesh.SamplePosition(k_CachedHit[0].point,
                                out var hit,
                                k_MaxNavMeshDistance,
                                NavMesh.AllAreas))
                    {
                        m_ServerCharacter.SendCharacterInputServerRpc(hit.position);
                        ClientMoveEvent?.Invoke(hit.position);
                    }
                }
            }

        }
    }
    void SendInput(ActionRequestData action)
    {
        m_ServerCharacter.RecvDoActionServerRPC(action);
    }
    void HandleFire(bool state)
    {
        if(state)
        {
            //m_ServerCharacter.Aim(m_InputManager.AimPosition, groundLayerMask);
            ActionRequestData actionRequestData = new ActionRequestData();

            actionRequestData.ActionID = m_ServerCharacter.GetStartAction().ActionID;
            actionRequestData.Direction = m_ServerCharacter.Aim(m_InputManager.GetMouseScreenPosition(), m_GroundLayerMask);
            SendInput(actionRequestData);
        }
    }
}
