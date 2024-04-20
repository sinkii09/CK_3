using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientInputSender : NetworkBehaviour
{
    [SerializeField]
    ServerCharacter m_ServerCharacter;

    public override void OnNetworkDespawn()
    {
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            ActionRequestData actionRequestData = new ActionRequestData();

            actionRequestData.ActionID = new ActionID();
            SendInput(actionRequestData);
        }
    }

    void SendInput(ActionRequestData action)
    {
        m_ServerCharacter.RecvDoActionServerRPC(action);
    }
}
