using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "GamePlay/Actions/Charged Launch Projectile Action")]
public partial class ChargedLaunchProjectileAction : LaunchProjectileAction
{
    private float m_StoppedChargingUpTime = 0;

    private bool m_HitByAttack = false;

    public override bool OnStart(ServerCharacter serverCharacter)
    {
        if (m_Data.TargetIds != null && m_Data.TargetIds.Length > 0)
        {
            NetworkObject initialTarget = NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_Data.TargetIds[0]];
            if (initialTarget)
            {
                serverCharacter.physicsWrapper.Transform.LookAt(initialTarget.transform.position);
            }
        }

        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return true;
    }
    public override void Reset()
    {
        base.Reset();
        m_ChargeEnded = false;
        m_StoppedChargingUpTime = 0;
        m_HitByAttack = false;
        m_Graphics.Clear();
    }

    public override bool OnUpdate(ServerCharacter clientCharacter)
    {
        if (m_StoppedChargingUpTime == 0 && GetPercentChargedUp() >= 1)
        {
            // we haven't explicitly stopped charging up... but we've reached max charge, so that implicitly stops us
            StopChargingUp(clientCharacter);
        }

        // we end as soon as we've stopped charging up (and have fired the projectile)
        return m_StoppedChargingUpTime == 0;
    }
    public override void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType)
    {
        if (activityType == GameplayActivity.AttackedByEnemy)
        {
            // if we get attacked while charging up, we don't actually get to shoot!
            m_HitByAttack = true;
            StopChargingUp(serverCharacter);
        }
        else if (activityType == GameplayActivity.StoppedChargingUp)
        {
            StopChargingUp(serverCharacter);
        }
    }
    public override void Cancel(ServerCharacter serverCharacter)
    {
        StopChargingUp(serverCharacter);
    }

    public override void End(ServerCharacter serverCharacter)
    {
        StopChargingUp(serverCharacter);
    }
    private void StopChargingUp(ServerCharacter parent)
    {
        if (m_StoppedChargingUpTime == 0)
        {
            m_StoppedChargingUpTime = Time.time;

            if (!string.IsNullOrEmpty(Config.Anim2))
            {
                parent.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim2);
            }
            parent.clientCharacter.RecvStopChargingUpClientRpc(GetPercentChargedUp());
            if (!m_HitByAttack)
            {
                LaunchProjectile(parent);
            }
        }
    }
    private float GetPercentChargedUp()
    {
        return ActionUtils.GetPercentChargedUp(m_StoppedChargingUpTime, TimeRunning, TimeStarted, Config.ExecTimeSeconds);
    }
    protected override ProjectileInfo GetProjectileInfo()
    {
        if (Config.Projectiles.Length == 0)
            throw new System.Exception($"Action {name} has no Projectiles!");

        int projectileIdx = (int)(GetPercentChargedUp() * (Config.Projectiles.Length - 1));
        return Config.Projectiles[projectileIdx];
    }
}
