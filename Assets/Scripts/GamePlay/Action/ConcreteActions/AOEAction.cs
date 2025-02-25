using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/AOE Action")]
public class AOEAction : Action
{
    const float k_MaxDistanceDivergence = 1;

    bool m_DidAoE;
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        float distanceAway = Vector3.Distance(serverCharacter.physicsWrapper.transform.position, Data.Position);
        if(distanceAway > Config.Range + k_MaxDistanceDivergence)
        {
            return ActionConclusion.Stop;
        }
        Data.TargetIds = new ulong[0];
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return ActionConclusion.Continue;
    }

    public override void Reset()
    {
        base.Reset();
        m_DidAoE = false;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if(TimeRunning >= Config.ExecTimeSeconds && !m_DidAoE)
        {
            m_DidAoE = true;
            PerformAoE(serverCharacter);
        }
        return ActionConclusion.Continue;
    }

    private void PerformAoE(ServerCharacter parent)
    {
        var colliders = Physics.OverlapSphere(m_Data.Position, Config.Radius, LayerMask.GetMask("NPCs"));
    
        for(var i = 0; i < colliders.Length; i++)
        {
            var enemy = colliders[i].GetComponent<IDamageable>();
            if(enemy != null)
            {
                enemy.ReceiveHP(parent, -Config.Damage);
            }
        }
    }

    public override bool OnStartClient(ClientCharacter clientCharacter)
    {
        base.OnStartClient(clientCharacter);
        if(Config.SpecialFX.Length > 0)
        {
            GameObject.Instantiate(Config.SpecialFX[0], Data.Position, Quaternion.identity);
        }
        return ActionConclusion.Stop;

    }
}
