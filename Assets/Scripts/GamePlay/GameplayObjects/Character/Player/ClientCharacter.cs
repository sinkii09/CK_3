using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;

public class ClientCharacter : NetworkBehaviour
{
    const float k_LerpTime = 0.08f;

    #region References
    [SerializeField]
    Animator m_ClientVisualsAnimator;
    public Animator OurAnimator => m_ClientVisualsAnimator;

    [SerializeField]
    VisualizationConfiguration m_VisualizationConfiguration;

    ServerCharacter m_ServerCharacter;
    public ServerCharacter serverCharacter => m_ServerCharacter;

    ClientActionPlayer m_ClientActionPlayer;

    public GameObject TargetReticulePrefab => m_VisualizationConfiguration.TargetReticule;
    public Material ReticuleHostileMat => m_VisualizationConfiguration.ReticuleHostileMat;
    public Material ReticuleFriendlyMat => m_VisualizationConfiguration.ReticuleFriendlyMat;
    #endregion

    #region Variables

    float m_CurrentSpeed;

    PositionLerper m_PositionLerper;

    RotationLerper m_RotationLerper;

    Vector3 m_LerpedPosition;

    Quaternion m_LerpedRotation;
    #endregion

    #region ClientRPC

    [ClientRpc]
    public void RecvDoActionClientRPC(ActionRequestData data)
    {
        ActionRequestData data1 = data;
        m_ClientActionPlayer.PlayAction(ref data1);
    }

    [ClientRpc]
    public void RecvCancelAllActionsClientRpc()
    {
        m_ClientActionPlayer.CancelAllActions();
    }

    [ClientRpc]
    public void RecvCancelActionsByPrototypeIDClientRpc(ActionID actionPrototypeID)
    {
        m_ClientActionPlayer.CancelAllActionsWithSamePrototypeID(actionPrototypeID);
    }
    [ClientRpc]
    public void RecvStopChargingUpClientRpc(float percentCharged)
    {
        m_ClientActionPlayer.OnStoppedChargingUp(percentCharged);
    }
    #endregion

    #region UnityCallBacks

    private void Awake()
    {
        enabled = false;
    }
    private void Update()
    {
        if(IsHost)
        {
            m_LerpedPosition = m_PositionLerper.LerpPosition(m_LerpedPosition,
                    serverCharacter.physicsWrapper.Transform.position);
            m_LerpedRotation = m_RotationLerper.LerpRotation(m_LerpedRotation,
                serverCharacter.physicsWrapper.Transform.rotation);
            transform.SetPositionAndRotation(m_LerpedPosition, m_LerpedRotation);
        }
        if(m_ClientVisualsAnimator)
        {
            OurAnimator.SetFloat(m_VisualizationConfiguration.SpeedVariableID, m_CurrentSpeed);
        }

         m_ClientActionPlayer.OnUpdate();
    }

    #endregion

    #region NetworkCallBacks
    public override void OnNetworkSpawn()
    {
        if(!IsClient || transform.parent == null)
        {
            return;
        }

        enabled = true;

        m_ClientActionPlayer = new ClientActionPlayer(this);

        m_ServerCharacter = GetComponentInParent<ServerCharacter>();

        m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
        OnMovementStatusChanged(MovementStatus.Normal, m_ServerCharacter.MovementStatus.Value);

        transform.SetPositionAndRotation(serverCharacter.physicsWrapper.Transform.position,
                                         serverCharacter.physicsWrapper.Transform.rotation);

        m_LerpedPosition = transform.position;
        m_LerpedRotation = transform.rotation;

        m_PositionLerper = new PositionLerper(serverCharacter.physicsWrapper.Transform.position, k_LerpTime);
        m_RotationLerper = new RotationLerper(serverCharacter.physicsWrapper.Transform.rotation, k_LerpTime);
        if (!m_ServerCharacter.IsNPC)
        {
            name = "AvatarGraphics" + m_ServerCharacter.OwnerClientId;
            if (m_ServerCharacter.TryGetComponent(out ClientAvatarGuidHandler clientAvatarGuidHandler))
            {
                m_ClientVisualsAnimator = clientAvatarGuidHandler.graphicsAnimator;
            }
            if (m_ServerCharacter.IsOwner)
            {
                ActionRequestData data = new ActionRequestData { ActionID = GameDataSource.Instance.GeneralTargetActionPrototype.ActionID };
                m_ClientActionPlayer.PlayAction(ref data);
                if (m_ServerCharacter.TryGetComponent(out ClientInputSender inputSender))
                {
                    // anticipated actions will only be played on non-host, owning clients
                    if (!IsServer)
                    {
                        inputSender.ActionInputEvent += OnActionInput;
                    }
                    inputSender.ClientMoveEvent += OnMoveInput;
                }
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if(m_ServerCharacter)
        {
            if(m_ServerCharacter.TryGetComponent(out ClientInputSender inputSender))
            {
                inputSender.ActionInputEvent -= OnActionInput;
                inputSender.ClientMoveEvent -= OnMoveInput;
            }
        }

        enabled = false;

        
    }

    #endregion

    private void OnMoveInput(Vector3 vector)
    {
        if (!IsAnimating())
        {
            //OurAnimator.SetTrigger(m_VisualizationConfiguration.AnticipateMoveTriggerID);
        }
    }

    private void OnActionInput(ActionRequestData data)
    {
        m_ClientActionPlayer.AnticipateAction(ref data);
    }

    void OnMovementStatusChanged(MovementStatus previousValue, MovementStatus newValue)
    {
        m_CurrentSpeed = GetVisualMovementSpeed(newValue);
    }
    float GetVisualMovementSpeed(MovementStatus movementStatus)
    {
        if(m_ServerCharacter.NetLifeState.LifeState.Value != LifeState.Alive)
        {
            return m_VisualizationConfiguration.SpeedDead;
        }
        switch (movementStatus)
        {
            case MovementStatus.Idle:
                return m_VisualizationConfiguration.SpeedIdle;
            case MovementStatus.Normal:
                return m_VisualizationConfiguration.SpeedNormal;
            case MovementStatus.Slowed:
                return m_VisualizationConfiguration.SpeedSlowed;
            case MovementStatus.Hasted:
                return m_VisualizationConfiguration.SpeedHasted;
            default:
                throw new Exception($"Unknown MovementStatus {movementStatus}");
        }
    }
    public bool IsAnimating()
    {
        if(OurAnimator.GetFloat(m_VisualizationConfiguration.SpeedVariableID) > 0.0)
        {
            return true;
        }
        for (int i = 0; i < OurAnimator.layerCount; i++)
        {
            if (OurAnimator.GetCurrentAnimatorStateInfo(i).tagHash != m_VisualizationConfiguration.BaseNodeTagID)
            {
                return true;
            }
        }
        return false;
    }
}
