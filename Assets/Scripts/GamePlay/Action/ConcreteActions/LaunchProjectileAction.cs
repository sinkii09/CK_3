using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Launch Projectile Action")]
public class LaunchProjectileAction : Action
{
    protected bool m_Launched = false;
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        serverCharacter.CheckAmountServerRpc();
        serverCharacter.physicsWrapper.Transform.forward = Data.Direction;
        serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
        serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
        return true;
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
    public override void Reset()
    {
        m_Launched = false;
        base.Reset();
    }

    protected virtual void LaunchProjectile(ServerCharacter parent)
    {
        if (!m_Launched)
        {
            m_Launched = true;
            var projectileInfo = GetProjectileInfo();
            NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.Prefab, projectileInfo.Prefab.transform.position, projectileInfo.Prefab.transform.rotation);
            networkObject.transform.forward = parent.physicsWrapper.transform.forward;
            networkObject.transform.position = parent.physicsWrapper.Transform.localToWorldMatrix.MultiplyPoint(networkObject.transform.position);
            networkObject.GetComponent<PhysicsProjectile>().Initialize(parent.NetworkObjectId, projectileInfo);
            networkObject.Spawn(true);
        }
    }
    protected virtual ProjectileInfo GetProjectileInfo()
    {
        foreach (var projectileInfo in Config.Projectiles)
        {
            if (projectileInfo.Prefab && projectileInfo.Prefab.GetComponent<PhysicsProjectile>())
                return projectileInfo;
        }
        throw new System.Exception($"Action {name} has no usable Projectiles!");
    }
    public override void End(ServerCharacter serverCharacter)
    {
        LaunchProjectile(serverCharacter);
    }
    public override void Cancel(ServerCharacter serverCharacter)
    {
        if (!string.IsNullOrEmpty(Config.Anim2))
        {
            serverCharacter.ServerAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim2);
        }
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return ActionConclusion.Continue;
    }
}
