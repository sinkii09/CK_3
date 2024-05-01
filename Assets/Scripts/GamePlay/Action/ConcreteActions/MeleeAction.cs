using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Melee Action")]
public partial class MeleeAction : Action
{
    private bool m_ExecutionFired;
    private ulong m_ProvisionalTarget;
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        ulong target = (Data.TargetIds != null && Data.TargetIds.Length > 0) ? Data.TargetIds[0] : serverCharacter.TargetId.Value;
        IDamageable foe = DetectFoe(serverCharacter, target);
        if (foe != null)
        {
            m_ProvisionalTarget = foe.NetworkObjectId;
            Data.TargetIds = new ulong[] { foe.NetworkObjectId };
        }
        if (Data.Direction != Vector3.zero)
        {
            serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        }
        serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return true;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (!m_ExecutionFired && (Time.time - TimeStarted) >= Config.ExecTimeSeconds)
        {
            m_ExecutionFired = true;
            var foe = DetectFoe(serverCharacter, m_ProvisionalTarget);
            if (foe != null)
            {
                foe.ReceiveHP(serverCharacter, -Config.Amount);  
            }
        }
        return true;
    }
    public override void Reset()
    {
        base.Reset();
        m_ExecutionFired = false;
        m_ProvisionalTarget = 0;
        m_ImpactPlayed = false;
        m_SpawnedGraphics = null;
    }
    private IDamageable DetectFoe(ServerCharacter parent, ulong foeHint = 0)
    {
        return GetIdealMeleeFoe(Config.IsFriendly ^ parent.IsNpc, parent.physicsWrapper.DamageCollider, Config.Range, foeHint);
    }

    public static IDamageable GetIdealMeleeFoe(bool isNPC, Collider ourCollider, float meleeRange, ulong preferredTargetNetworkId)
    {
        RaycastHit[] results;
        int numResults = ActionUtils.DetectMeleeFoe(isNPC, ourCollider, meleeRange, out results);

        IDamageable foundFoe = null;

        for (int i = 0; i < numResults; i++)
        {
            var damageable = results[i].collider.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsDamageable() &&
                (damageable.NetworkObjectId == preferredTargetNetworkId || foundFoe == null))
            {
                foundFoe = damageable;
            }
        }
        return foundFoe;
    }
}
