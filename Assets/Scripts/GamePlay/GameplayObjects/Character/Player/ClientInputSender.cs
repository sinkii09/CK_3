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

    bool m_MoveRequest;

    readonly RaycastHit[] k_CachedHit = new RaycastHit[4];

    RaycastHitComparer m_RaycastHitComparer;

    public enum SkillTriggerStyle
    {
        None,      
        MouseClick, 
        Keyboard,  
        KeyboardRelease, 
        UI,          
        UIRelease,   
    }
    bool IsReleaseStyle(SkillTriggerStyle style)
    {
        return style == SkillTriggerStyle.KeyboardRelease || style == SkillTriggerStyle.UIRelease;
    }

    struct ActionRequest
    {
        public SkillTriggerStyle TriggerStyle;
        public ActionID RequestedActionID;
        public ulong TargetId;
    }
    readonly ActionRequest[] m_ActionRequests = new ActionRequest[5];

    int m_ActionRequestCount;

    public ActionState baseAttack { get; private set; }

    LayerMask m_GroundLayerMask;
    LayerMask m_ActionLayerMask;

    BaseActionInput m_CurrentSkillInput;

    #endregion

    #region Events

    public event Action<Vector3> ClientMoveEvent;

    public event Action<ActionRequestData> ActionInputEvent;

    #endregion

    #region Ref
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField]
    InputManager m_InputManager;

    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;
    private bool canDoAction;

    CharacterClass CharacterClass => m_ServerCharacter.CharacterClass;

    public NetworkVariable<ulong> HeldNetworkItem { get; } = new NetworkVariable<ulong>();
    #endregion
    public override void OnNetworkSpawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
        m_InputManager.MoveEvent += HandleMovement;
        m_InputManager.JumpEvent += HandleJump;
        m_ServerCharacter.HeldItem.OnValueChanged += OnHeldNetworkItemChanged;

        m_GroundLayerMask = LayerMask.GetMask(new[] { "Ground" });
        m_ActionLayerMask = LayerMask.GetMask(new[] { "Player"," NPCs" , "Ground" });



        if (CharacterClass.BaseAttack && GameDataSource.Instance.TryGetActionPrototypeByID(CharacterClass.BaseAttack.ActionID, out var baseAttack))
        {
            this.baseAttack = new ActionState()
            {
                actionID = baseAttack.ActionID,
                selectable = true,
            };
        }
    }

    public override void OnNetworkDespawn()
    {
        if(m_ServerCharacter)
        {
            m_InputManager.MoveEvent -= HandleMovement;
            m_InputManager.JumpEvent -= HandleJump;
            m_ServerCharacter.HeldItem.OnValueChanged -= OnHeldNetworkItemChanged;
        }
    }
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && m_CurrentSkillInput == null)
        {
            if(m_InputManager.IsLeftMouseButtonDownThisFrame())
            {
                RequestAction(baseAttack.actionID, SkillTriggerStyle.MouseClick);
            }
        }
    }
    private void FixedUpdate()
    {
        for(int i = 0; i < m_ActionRequestCount; ++i)
        {
            if(m_CurrentSkillInput != null)
            {
                if (IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
                {
                    m_CurrentSkillInput.OnReleaseKey();
                }
            }
            else if (!IsReleaseStyle(m_ActionRequests[i].TriggerStyle))
            {
                var actionPrototype = GameDataSource.Instance.GetActionPrototypeByID(m_ActionRequests[i].RequestedActionID);
                if(actionPrototype.Config.ActionInput!=null)
                {
                    var skillPlayer = Instantiate(actionPrototype.Config.ActionInput);
                    skillPlayer.Initiate(m_ServerCharacter, m_PhysicsWrapper.transform.position, actionPrototype.ActionID, SendInput, FinishSkill);
                    m_CurrentSkillInput = skillPlayer;
                }
                else
                {
                    PerformAction(actionPrototype.ActionID, m_ActionRequests[i].TriggerStyle, m_ActionRequests[i].TargetId);
                }
            }
        }
        m_ActionRequestCount = 0;

        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }
    }
    void HandleJump(bool canJump)
    {
        m_ServerCharacter.SendCharacterInputServerRpc(Vector3.zero,canJump);
    }
    void HandleMovement(Vector2 moveDir)
    {
        m_ServerCharacter.SendCharacterInputServerRpc(moveDir);
    }
    void SendInput(ActionRequestData action)
    {
        ActionInputEvent?.Invoke(action);
        m_ServerCharacter.RecvDoActionServerRPC(action);
    }
    void PerformAction(ActionID actionID, SkillTriggerStyle triggerStyle, ulong targetId = 0)
    {
        Transform hitTransform = null;
        if(targetId != 0)
        {
            NetworkObject targetNetworkObject;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out targetNetworkObject))
            {
                hitTransform = targetNetworkObject.transform;
            }
        }
        else
        {
            int numHits = 0;
            var ray = Camera.main.ScreenPointToRay(m_InputManager.GetMouseScreenPosition());
            numHits = Physics.RaycastNonAlloc(ray, k_CachedHit, k_MouseInputRaycastDistance, m_ActionLayerMask);

            int networkedHitIndex = -1;
            for (int i = 0; i < numHits; i++)
            {
                if (k_CachedHit[i].transform.GetComponentInParent<NetworkObject>())
                {
                    networkedHitIndex = i;
                    break;
                }
                hitTransform = networkedHitIndex >= 0 ? k_CachedHit[networkedHitIndex].transform : null;
            }
        }
        var data = new ActionRequestData();
        PopulateSkillRequest(k_CachedHit[0].point, actionID, ref data);
        SendInput(data);
    }
    void PopulateSkillRequest(Vector3 hitPoint, ActionID actionID, ref ActionRequestData resultData)
    {
        resultData.ActionID = actionID;
        var actionConfig = GameDataSource.Instance.GetActionPrototypeByID(actionID).Config;
        resultData.ShouldClose = true;

        Vector3 offset = hitPoint - m_PhysicsWrapper.Transform.position;
        offset.y = 0;
        Vector3 direction = offset.normalized;

        switch (actionConfig.Logic)
        {
            case ActionLogic.LaunchProjectile:
                resultData.Direction = direction;
                resultData.ShouldClose = false;
                resultData.CancelMovement = false;
                return;
            case ActionLogic.Toss:
                resultData.Direction = direction;
                resultData.ShouldClose = false;
                resultData.CancelMovement = false;
                return;
            case ActionLogic.Melee:
                resultData.Direction = direction;
                return;
            case ActionLogic.Dash:
                resultData.Position = hitPoint;
                resultData.CancelMovement = true;
                return;
            case ActionLogic.Target:
                resultData.ShouldClose = false;
                return;
            case ActionLogic.CurveLaunchProjectile:
                resultData.Position = hitPoint;
                resultData.Direction = direction;
                resultData.ShouldClose = false;
                resultData.CancelMovement = true;
                return;
            case ActionLogic.Jump: 
                resultData.Direction = direction;
                resultData.CancelMovement = true;
                return;
        }
    }
    
    public void RequestAction(ActionID actionID, SkillTriggerStyle triggerStyle, ulong targetID = 0)
    {
        if(m_ActionRequestCount < m_ActionRequests.Length)
        {
            m_ActionRequests[m_ActionRequestCount].RequestedActionID = actionID;
            m_ActionRequests[m_ActionRequestCount].TriggerStyle = triggerStyle;
            m_ActionRequests[m_ActionRequestCount].TargetId = targetID;
            m_ActionRequestCount++;
        }
    }

    private void OnHeldNetworkItemChanged(ulong previousValue, ulong newValue)
    {
        UpdateAction();
    }

    private void UpdateAction()
    {
        // lay duoc id cua action trong item
        var isHaveItem = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_ServerCharacter.HeldItem.Value, out var itemObject);
        if(isHaveItem)
        {
            baseAttack.actionID = itemObject.GetComponent<Item>().m_ItemConfig.m_Action.ActionID;
            m_ServerCharacter.SetAmountServerRpc(baseAttack.actionID);
            itemObject.GetComponent<Item>().DeSpawnObject();
        }
        else
        {
            baseAttack.actionID = CharacterClass.BaseAttack.ActionID;
        }
        m_ServerCharacter.clientCharacter.RecvUpdateActionVisualClientRpc(baseAttack.actionID);
    }

    void FinishSkill()
    {
        m_CurrentSkillInput = null;
    }
    public class ActionState
    {
        public ActionID actionID { get; internal set; }

        public bool selectable { get; internal set; }

        internal void SetActionState(ActionID newActionID, bool isSelectable = true)
        {
            actionID = newActionID;
            selectable = isSelectable;
        }
    }
}
