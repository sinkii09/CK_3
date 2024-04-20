using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;


public class ServerCharacter : NetworkBehaviour
{
    [FormerlySerializedAs("m_ClientVisualization")]
    [SerializeField] ClientCharacter m_ClientCharacter;

    public ClientCharacter clientCharacter => m_ClientCharacter;

    [SerializeField]
    ServerCharacterMovement m_Movement;

    public ServerCharacterMovement Movement => m_Movement;

    [SerializeField]
    PhysicsWrapper m_PhysicsWrapper;

    public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

    public NetworkVariable<ulong> HeldNetworkObject { get; } = new NetworkVariable<ulong>();

    public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();

    private ServerActionPlayer m_ServerActionPlayer;
    public ServerActionPlayer ActionPlayer => m_ServerActionPlayer;

    [SerializeField] private Action m_StartingAction;
    private void Awake()
    {
        m_ServerActionPlayer = new ServerActionPlayer(this);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { enabled = false; }
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
    public void SendCharacterInputServerRpc()
    {

    }
    [ServerRpc]
    public void RecvDoActionServerRPC(ActionRequestData data)
    {
        ActionPlayer.OnGameplayActivity(Action.GameplayActivity.UsingAttackAction);
        
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

}
