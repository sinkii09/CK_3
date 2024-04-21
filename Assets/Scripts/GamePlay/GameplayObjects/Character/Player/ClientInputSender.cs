using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientInputSender : NetworkBehaviour
{
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    [SerializeField]
    InputManager m_InputManager;

    [SerializeField]
    LayerMask groundLayerMask;
    public override void OnNetworkSpawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
        m_InputManager.FireEvent += HandleFire;
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
            actionRequestData.Direction = m_ServerCharacter.Aim(m_InputManager.AimPosition, groundLayerMask);
            SendInput(actionRequestData);
        }
    }
}
