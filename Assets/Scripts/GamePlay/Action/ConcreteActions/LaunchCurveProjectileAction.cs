using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Launch Curve Projectile Action")]
public class LaunchCurveProjectileAction : LaunchProjectileAction
{
    const float k_MaxDistanceDivergence = 1;
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        float distanceAway = Vector3.Distance(serverCharacter.physicsWrapper.transform.position, Data.Position);
        if (distanceAway > Config.Range + k_MaxDistanceDivergence)
        {
            return ActionConclusion.Stop;
        }
        serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return ActionConclusion.Continue;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        if (!m_Launched)
        {
            
            serverCharacter.physicsWrapper.Transform.forward = Vector3.Lerp(serverCharacter.physicsWrapper.Transform.forward, Data.Direction, Time.deltaTime * 10);
            LaunchProjectile(serverCharacter);
        }
        return true;
    }

    protected override ProjectileInfo GetProjectileInfo()
    {

        foreach (var projectileInfo in Config.Projectiles)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<CurvePhysicsProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }

    protected override void LaunchProjectile(ServerCharacter parent)
    {
        if (!m_Launched)
        {
            m_Launched = true;
            var projectileInfo = GetProjectileInfo();
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
            networkObject.transform.forward = parent.physicsWrapper.transform.forward;
            networkObject.transform.position = parent.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
            networkObject.GetComponent<CurvePhysicsProjectile>().Initialize(Data.Position, projectileInfo);
            networkObject.Spawn(true);
        }
    }
}
