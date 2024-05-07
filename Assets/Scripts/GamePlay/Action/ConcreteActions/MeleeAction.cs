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

        // snap to face the right direction
        if (Data.Direction != Vector3.zero)
        {
            serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        }

        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
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

    public override bool OnUpdate(ServerCharacter clientCharacter)
    {
        if (!m_ExecutionFired && (Time.time - TimeStarted) >= Config.ExecTimeSeconds)
        {
            m_ExecutionFired = true;

            var foe = DetectFoe(clientCharacter, m_ProvisionalTarget);
            if (foe != null)
            {
                Debug.Log(-Config.Amount);
                foe.ReceiveHP(clientCharacter, -Config.Amount);
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the ServerCharacter of the foe we hit, or null if none found.
    /// </summary>
    /// <returns></returns>
    private IDamageable DetectFoe(ServerCharacter parent, ulong foeHint = 0)
    {
        return GetIdealMeleeFoe(Config.IsFriendly ^ parent.IsNPC, parent.physicsWrapper.DamageCollider, Config.Range, foeHint);
    }

    /// <summary>
    /// Utility used by Actions to perform Melee attacks. Performs a melee hit-test
    /// and then looks through the results to find an alive target, preferring the provided
    /// enemy.
    /// </summary>
    /// <param name="isNPC">true if the attacker is an NPC (and therefore should hit PCs). False for the reverse.</param>
    /// <param name="ourCollider">The collider of the attacking GameObject.</param>
    /// <param name="meleeRange">The range in meters to check for foes.</param>
    /// <param name="preferredTargetNetworkId">The NetworkObjectId of our preferred foe, or 0 if no preference</param>
    /// <returns>ideal target's IDamageable, or null if no valid target found</returns>
    public static IDamageable GetIdealMeleeFoe(bool isNPC, Collider ourCollider, float meleeRange, ulong preferredTargetNetworkId)
    {
        RaycastHit[] results;
        int numResults = ActionUtils.DetectMeleeFoe(isNPC, ourCollider, meleeRange, out results);

        IDamageable foundFoe = null;

        //everything that got hit by the raycast should have an IDamageable component, so we can retrieve that and see if they're appropriate targets.
        //we always prefer the hinted foe. If he's still in range, he should take the damage, because he's who the client visualization
        //system will play the hit-react on (in case there's any ambiguity).
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
