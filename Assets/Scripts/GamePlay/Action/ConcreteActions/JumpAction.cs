using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Jump Action")]
public class JumpAction : Action
{
    private Vector3 m_JumpDirection;
    private bool m_Jumped;

    public override bool OnStart(ServerCharacter serverCharacter)
    {
        m_JumpDirection = Data.Direction;

        serverCharacter.physicsWrapper.Transform.LookAt(m_JumpDirection);
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return ActionConclusion.Continue;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (!m_Jumped)
        {
            m_Jumped = true;
            Jump(serverCharacter);
        }
        return ActionConclusion.Continue;
    }

    public override void Cancel(ServerCharacter serverCharacter)
    {
        base.Cancel(serverCharacter);
    }

    public override void End(ServerCharacter serverCharacter)
    {
        base.End(serverCharacter);
    }
    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        if (m_Jumped) { return ActionConclusion.Stop; }
        return ActionConclusion.Continue;
    }

    public override void Reset()
    {
        base.Reset();
        m_JumpDirection = default;
        m_Jumped = false;
    }

    void Jump(ServerCharacter serverCharacter)
    {
        serverCharacter.GetComponent<Rigidbody>().AddForce((serverCharacter.physicsWrapper.transform.forward * 20f) + (serverCharacter.physicsWrapper.transform.up * 40f), ForceMode.Impulse);
    }

}
