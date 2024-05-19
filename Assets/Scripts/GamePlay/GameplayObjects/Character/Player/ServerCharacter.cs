using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;


public class ServerCharacter : NetworkBehaviour, ITargetable
{
    #region References
    [Header("Client Handler")]
    [FormerlySerializedAs("m_ClientVisualization")]
    [SerializeField] ClientCharacter m_ClientCharacter;
    public ClientCharacter clientCharacter => m_ClientCharacter;

    [Header("Movement Handler")]
    [SerializeField]
    NewCharacterMovement m_Movement;
    public NewCharacterMovement Movement => m_Movement;

    [Header("Physics Handler")]
    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;
    public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

    [Header("Animation Handler")]
    [SerializeField]
    ServerAnimationHandler m_ServerAnimationHandler;
    public ServerAnimationHandler ServerAnimationHandler => m_ServerAnimationHandler;

    [Header("Damage Handler")]
    [SerializeField]
    DamageReceiver m_DamageReceiver;

    [Header("Class Handler (AutoSet)")]
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

    public NetworkHealthState NetHealthState { get; private set; }
    public NetworkLifeState NetLifeState { get; private set; }


    /// <summary>
    /// Server Action System Handler
    /// </summary>
    private ServerActionPlayer m_ServerActionPlayer;
    public ServerActionPlayer ActionPlayer => m_ServerActionPlayer;

    private NetworkAvatarGuidState m_State;

    #endregion

    #region Variables
    public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>();
    public NetworkVariable<ulong> HeldItem { get; } = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();

    public int HitPoints
    {
        get => NetHealthState.HitPoints.Value;
        private set => NetHealthState.HitPoints.Value = value;
    }
    public LifeState LifeState
    {
        get => NetLifeState.LifeState.Value;
        private set => NetLifeState.LifeState.Value = value;
    }
    public CharacterTypeEnum CharacterType => CharacterClass.CharacterType;
    public bool IsValidTarget => LifeState != LifeState.Dead;
    public bool IsNPC => CharacterClass.IsNpc;

    [Header("Preset Variables")]
    [SerializeField]
    [Tooltip("If set to false, an NPC character will be denied its brain (won't attack or chase players)")]
    private bool m_BrainEnabled = true;

    [SerializeField]
    [Tooltip("Setting negative value disables destroying object after it is killed.")]
    private float m_KilledDestroyDelaySeconds = 3.0f;

    [SerializeField] private Action m_StartingAction;
    #endregion

    #region Unity CallBacks
    private void Awake()
    {
        m_ServerActionPlayer = new ServerActionPlayer(this);
        m_State = GetComponent<NetworkAvatarGuidState>();
        NetLifeState = GetComponent<NetworkLifeState>();
        NetHealthState = GetComponent<NetworkHealthState>();
    }
    private void Update()
    {
        m_ServerActionPlayer.OnUpdate();
    }
    #endregion

    #region Network CallBacks
    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            enabled = false;     
        }
        else
        {
            NetLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            m_DamageReceiver.DamageReceived += ReceiveHP;
            m_DamageReceiver.CollisionEntered += CollisionEntered;

            if (m_StartingAction != null)
            {
                var startingAction = new ActionRequestData() { ActionID = m_StartingAction.ActionID };
                PlayAction(ref startingAction);
            }
            InitializeHitPoints();
        }
    }

    public override void OnNetworkDespawn()
    {
        NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
        if (m_DamageReceiver)
        {
            m_DamageReceiver.DamageReceived -= ReceiveHP;
            m_DamageReceiver.CollisionEntered -= CollisionEntered;
        }
    }
    #endregion

    #region ServerRPC
    [ServerRpc]
    public void SendCharacterInputServerRpc(Vector3 movementTarget, bool canJump = false)
    {
        if (LifeState == LifeState.Alive/* && !m_Movement.IsPerformingForcedMovement()*/)
        {
            if (m_ServerActionPlayer.GetActiveActionInfo(out ActionRequestData data))
            {
                if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
                {
                    m_ServerActionPlayer.ClearActions(false);
                }
            }
            m_ServerActionPlayer.CancelRunningActionsByLogic(ActionLogic.Target, true);
            if (canJump)
            {
                m_Movement.SetJump();
                return;
            }
            m_Movement.SetMoveDirection(movementTarget);
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
    #endregion

    #region Others
    public void PlayAction(ref ActionRequestData action)
    {
        if (LifeState == LifeState.Alive/* && !m_Movement.IsPerformingForcedMovement()*/)
        {
            if (action.CancelMovement)
            {
                m_Movement.CancelMove();
            }
            m_ServerActionPlayer.PlayAction(ref action);
        }
    }
    public Action GetStartAction()
    {
        return m_StartingAction;
    }
    private void OnLifeStateChanged(LifeState prevLifeState, LifeState lifeState)
    {
        if (lifeState != LifeState.Alive)
        {
            m_ServerActionPlayer.ClearActions(true);
            m_Movement.CancelMove();
        }
    }
    IEnumerator KilledDestroyProcess()
    {
        yield return new WaitForSeconds(m_KilledDestroyDelaySeconds);

        if (NetworkObject != null)
        {
            NetworkObject.Despawn(true);
        }
    }
    void ReceiveHP(ServerCharacter inflicter, int HP)
    {
        if (HP > 0)
        {
            m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.Healed);
            float healingMod = m_ServerActionPlayer.GetBuffedValue(Action.BuffableValue.PercentHealingReceived);
            HP = (int)(HP * healingMod);
        }
        else
        {
            m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.AttackedByEnemy);
            float damageMod = m_ServerActionPlayer.GetBuffedValue(Action.BuffableValue.PercentDamageReceived);
            HP = (int)(HP * damageMod);

            ServerAnimationHandler.NetworkAnimator.SetTrigger("HitReact1");
        }
        HitPoints = Mathf.Clamp(HitPoints + HP, 0, CharacterClass.BaseHP);

        if (HitPoints <= 0)
        {
            if (IsNPC)
            {
                if (m_KilledDestroyDelaySeconds >= 0.0f && LifeState != LifeState.Dead)
                {
                    StartCoroutine(KilledDestroyProcess());
                }
                LifeState = LifeState.Dead;
            }
            else
            {
                LifeState = LifeState.Fainted;
            }

            m_ServerActionPlayer.ClearActions(false);
        }
    }
    void InitializeHitPoints()
    {
        HitPoints = CharacterClass.BaseHP;
        //TODO:
        //if (!IsNpc)
        //{
        //    SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
        //    if (sessionPlayerData is { HasCharacterSpawned: true })
        //    {
        //        HitPoints = sessionPlayerData.Value.CurrentHitPoints;
        //        if (HitPoints <= 0)
        //        {
        //            LifeState = LifeState.Fainted;
        //        }
        //    }
        //}
    }
    void CollisionEntered(Collision collision)
    {
        if (m_ServerActionPlayer != null)
        {
            m_ServerActionPlayer.CollisionEntered(collision);
        }
    }
    #endregion
}
