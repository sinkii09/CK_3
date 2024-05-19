using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Dash Attack Action")]
public class DashAction : Action
{
    private Vector3 m_TargetSpot;

    private bool m_Dashed;

    public override bool OnStart(ServerCharacter serverCharacter)
    {
        m_TargetSpot = ActionUtils.GetDashDestination(serverCharacter.physicsWrapper.Transform, Data.Position, true, Config.Range, Config.Range);

        serverCharacter.physicsWrapper.Transform.LookAt(m_TargetSpot);

        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);

        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);

        return ActionConclusion.Continue;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        return ActionConclusion.Continue;
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        if (m_Dashed) { return ActionConclusion.Stop; }
        return ActionConclusion.Continue;
    }

    public override void Reset()
    {
        base.Reset();
        m_TargetSpot = default;
        m_Dashed = false;
    }

    public override void Cancel(ServerCharacter serverCharacter)
    {
        if (!string.IsNullOrEmpty(Config.OtherAnimatorVariable))
        {
            serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.OtherAnimatorVariable);
        }
        serverCharacter.clientCharacter.RecvCancelActionsByPrototypeIDClientRpc(ActionID);
    }

    public override void End(ServerCharacter serverCharacter)
    {
        if (!string.IsNullOrEmpty(Config.Anim2))
        {
            serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim2);
        }

        //serverCharacter.Movement.Teleport(m_TargetSpot);

        if(serverCharacter.CharacterType.Equals(CharacterTypeEnum.Rogue))
        {
            //TODO: rougeAttack
        }
    }

    public override void BuffValue(BuffableValue buffType, ref float buffedValue)
    {
        if (TimeRunning >= Config.ExecTimeSeconds && buffType == BuffableValue.PercentDamageReceived)
        {
            buffedValue = 0;
        }
    }
}
