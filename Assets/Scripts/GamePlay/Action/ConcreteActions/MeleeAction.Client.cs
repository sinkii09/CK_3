using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class MeleeAction
{
    private bool m_ImpactPlayed;

    private const float k_RangePadding = 3f;

    private List<SpecialFXGraphic> m_SpawnedGraphics = null;
    public override bool OnStartClient(ClientCharacter clientCharacter)
    {
        base.OnStartClient(clientCharacter);

       
        if (Data.TargetIds != null
            && Data.TargetIds.Length > 0
            && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(Data.TargetIds[0], out var targetNetworkObj)
            && targetNetworkObj != null)
        {
            float padRange = Config.Range + k_RangePadding;

            Vector3 targetPosition;
            if (PhysicsWrapper.TryGetPhysicsWrapper(Data.TargetIds[0], out var physicsWrapper))
            {
                targetPosition = physicsWrapper.Transform.position;
            }
            else
            {
                targetPosition = targetNetworkObj.transform.position;
            }

            if ((clientCharacter.transform.position - targetPosition).sqrMagnitude < (padRange * padRange))
            {
                m_SpawnedGraphics = InstantiateSpecialFXGraphics(physicsWrapper ? physicsWrapper.Transform : targetNetworkObj.transform, true);
            }
        }

        return true;
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return ActionConclusion.Continue;
    }

    public override void OnAnimEventClient(ClientCharacter clientCharacter, string id)
    {
        if (id == "impact" && !m_ImpactPlayed)
        {
            PlayHitReact(clientCharacter);
        }
    }

    public override void EndClient(ClientCharacter clientCharacter)
    {
        PlayHitReact(clientCharacter);
        base.EndClient(clientCharacter);
    }

    public override void CancelClient(ClientCharacter clientCharacter)
    {
        if (m_SpawnedGraphics != null)
        {
            foreach (var spawnedGraphic in m_SpawnedGraphics)
            {
                if (spawnedGraphic)
                {
                    spawnedGraphic.Shutdown();
                }
            }
        }
    }

    void PlayHitReact(ClientCharacter parent)
    {
        if (m_ImpactPlayed) { return; }

        m_ImpactPlayed = true;

        if (NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (Data.TargetIds != null &&
            Data.TargetIds.Length > 0 &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(Data.TargetIds[0], out var targetNetworkObj)
            && targetNetworkObj != null)
        {
            float padRange = Config.Range + k_RangePadding;

            Vector3 targetPosition;
            if (PhysicsWrapper.TryGetPhysicsWrapper(Data.TargetIds[0], out var movementContainer))
            {
                targetPosition = movementContainer.Transform.position;
            }
            else
            {
                targetPosition = targetNetworkObj.transform.position;
            }

            if ((parent.transform.position - targetPosition).sqrMagnitude < (padRange * padRange))
            {
                if (targetNetworkObj.NetworkObjectId != parent.NetworkObjectId)
                {
                    string hitAnim = Config.ReactAnim;
                    if (string.IsNullOrEmpty(hitAnim)) { hitAnim = k_DefaultHitReact; }

                    if (targetNetworkObj.TryGetComponent<ServerCharacter>(out var serverCharacter)
                        && serverCharacter.clientCharacter != null
                        && serverCharacter.clientCharacter.OurAnimator)
                    {
                        serverCharacter.clientCharacter.OurAnimator.SetTrigger(hitAnim);
                    }
                }
            }
        }
    }
}
