using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;


public class ServerCharacter : NetworkBehaviour
{
    [FormerlySerializedAs("m_ClientVisualization")]
    [SerializeField] ClientCharacter m_ClientCharacter;

    public ClientCharacter clientCharacter => m_ClientCharacter;

    public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();

    [SerializeField]
    ServerCharacterMovement m_Movement;

    public ServerCharacterMovement Movement => m_Movement;


    public NetworkLifeState NetLifeState { get; private set; }

    public LifeState LifeState
    {
        get => NetLifeState.LifeState.Value;
        private set => NetLifeState.LifeState.Value = value;
    }

    [SerializeField]
    CharacterClass m_CharacterClass;

    public CharacterClass CharacterClass
    {
        get 
        { 
            if (m_CharacterClass == null)
            {
                m_CharacterClass = m_State.RegisteredAvatar.characterClass;
            }
            return m_CharacterClass; 
        }
        set { m_CharacterClass = value; }
    }
    public CharacterTypeEnum CharacterType => CharacterClass.CharacterType;
    
    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;

    public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

    [SerializeField]
    ServerAnimationHandler m_ServerAnimationHandler;

    public ServerAnimationHandler serverAnimationHandler => m_ServerAnimationHandler;

    public NetworkVariable<ulong> HeldNetworkObject { get; } = new NetworkVariable<ulong>();

    public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();

    private ServerActionPlayer m_ServerActionPlayer;
    public ServerActionPlayer ActionPlayer => m_ServerActionPlayer;

    [SerializeField] private Action m_StartingAction;

    public bool IsNpc => CharacterClass.IsNpc;

    NetworkAvatarGuidState m_State;

    private void Awake()
    {
        m_ServerActionPlayer = new ServerActionPlayer(this);
        m_State = GetComponent<NetworkAvatarGuidState>();
        NetLifeState = GetComponent<NetworkLifeState>();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            enabled = false;     
        }
        else
        {
            if (m_StartingAction != null)
            {
                var startingAction = new ActionRequestData() { ActionID = m_StartingAction.ActionID };
                PlayAction(ref startingAction);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        
    }
    private void Update()
    {
        m_ServerActionPlayer.OnUpdate();
    }

    [ServerRpc]
    public void SendCharacterInputServerRpc(Vector3 movementTarget)
    {
        if (LifeState == LifeState.Alive && !m_Movement.IsPerformingForcedMovement())
        {
            if (m_ServerActionPlayer.GetActiveActionInfo(out ActionRequestData data))
            {
                if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
                {
                    m_ServerActionPlayer.ClearActions(false);
                }
            }
            m_ServerActionPlayer.CancelRunningActionsByLogic(ActionLogic.Target, true);
            m_Movement.SetMovementTarget(movementTarget);
        }
    }
    [ServerRpc]
    public void RecvDoActionServerRPC(ActionRequestData data)
    {
        ActionRequestData data1 = data;
        if (!GameDataSource.Instance.GetActionPrototypeByID(data1.ActionID).Config.IsFriendly)
        {
            ActionPlayer.OnGameplayActivity(Action.GameplayActivity.UsingAttackAction);
        }
        PlayAction(ref data1);
    }
    [ServerRpc]
    public void RecvStopChargingUpServerRpc()
    {
        m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.StoppedChargingUp);
    }

    public void PlayAction(ref ActionRequestData action)
    {
        if(action.CancelMovement)
        {
            m_Movement.CancelMove();
        }
        m_ServerActionPlayer.PlayAction(ref action);
    }
    public Action GetStartAction()
    {
        return m_StartingAction;
    }
    public Vector3 Aim(Vector2 lookDir, LayerMask layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(lookDir);
        if (Physics.Raycast(ray, out RaycastHit hitinfo, layerMask))
        {
            return  new Vector3(hitinfo.point.x - transform.position.x,0, hitinfo.point.z - transform.position.z).normalized;
        }
        return Vector3.zero;
    }
}
