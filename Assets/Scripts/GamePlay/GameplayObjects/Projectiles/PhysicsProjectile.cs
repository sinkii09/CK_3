using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsProjectile : NetworkBehaviour
{
    const int k_MaxCollisions = 4;
    const float k_WallLingerSec = 2f;
    const float k_EnemyLingerSec = 0.2f;
    const float k_LerpTime = 0.1f;

    [SerializeField]
    SphereCollider m_OurCollider;
    
    [SerializeField]
    SpecialFXGraphic m_OnHitParticlePrefab;

    [SerializeField]
    Transform m_Visualization;

    [SerializeField]
    TrailRenderer m_TrailRenderer;

    bool m_Started;
    bool m_IsDead;

    ulong m_SpawnerId;

    ProjectileInfo m_ProjectileInfo;
    PositionLerper m_PositionLerper;

    float m_DestroyAtSec;


    Collider[] m_CollisionCache = new Collider[k_MaxCollisions];

    int m_CollisionMask;
    int m_BlockerMask;
    int m_NpcLayer;

    List<GameObject> m_HitTargets = new List<GameObject>();


    public void Initialize(ulong creatorsNetworkObjectId, in ProjectileInfo projectileInfo)
    {
        m_SpawnerId = creatorsNetworkObjectId;
        m_ProjectileInfo = projectileInfo;
    }

    public override void OnNetworkSpawn()
    {
       if(IsServer)
       {
            m_Started = true;
            m_IsDead = false;

            m_HitTargets = new List<GameObject>();
            m_DestroyAtSec = Time.fixedTime + (m_ProjectileInfo.Range / m_ProjectileInfo.Speed);

            m_CollisionMask = LayerMask.GetMask(new[] { "PCs", "Environment" });
            m_BlockerMask = LayerMask.GetMask(new[] {  "Environment" });
            m_NpcLayer = LayerMask.NameToLayer("NPCs");

            if (IsClient)
            {
                m_TrailRenderer.Clear();

                m_Visualization.parent = null;

                m_PositionLerper = new PositionLerper(transform.position, k_LerpTime);
            }
       }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            m_Started = false;
        }

        if (IsClient)
        {
            m_TrailRenderer.Clear();
            m_Visualization.parent = transform;
        }
    }
    private void Update()
    {
        if(IsClient)
        {
            if(IsHost)
            {
                m_Visualization.position = m_PositionLerper.LerpPosition(m_Visualization.position,
                        transform.position);
            }
            else
            {
                m_Visualization.position = transform.position;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!m_Started || !IsServer)
        {
            return;
        }
        if(m_DestroyAtSec < Time.fixedTime)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Despawn();
            return;
        }
        var displacement = transform.forward * (m_ProjectileInfo.Speed * Time.fixedDeltaTime);
        transform.position += displacement;

        if (!m_IsDead)
        {
            DetectCollisions();
        }
    }
    void DetectCollisions()
    {
        var position = transform.localToWorldMatrix.MultiplyPoint(m_OurCollider.center);
        var numCollisions = Physics.OverlapSphereNonAlloc(position, m_OurCollider.radius, m_CollisionCache, m_CollisionMask);
        for (int i = 0; i < numCollisions; i++)
        {
            int layerTest = 1 << m_CollisionCache[i].gameObject.layer;
            if ((layerTest & m_BlockerMask) != 0)
            {
                m_ProjectileInfo.Speed = 0;
                m_IsDead = true;
                m_DestroyAtSec = Time.fixedTime + k_WallLingerSec;
                return;
            }

            if (m_CollisionCache[i].gameObject.layer == m_NpcLayer && !m_HitTargets.Contains(m_CollisionCache[i].gameObject))
            {
                m_HitTargets.Add(m_CollisionCache[i].gameObject);

                if (m_HitTargets.Count >= m_ProjectileInfo.MaxVictims)
                {
                    // we've hit all the enemies we're allowed to! So we're done
                    m_DestroyAtSec = Time.fixedTime + k_EnemyLingerSec;
                    m_IsDead = true;
                }

                //all NPC layer entities should have one of these.
                var targetNetObj = m_CollisionCache[i].GetComponentInParent<NetworkObject>();
                if (targetNetObj)
                {
                    RecvHitEnemyClientRPC(targetNetObj.NetworkObjectId);

                    //retrieve the person that created us, if he's still around.
                    NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_SpawnerId, out var spawnerNet);
                    var spawnerObj = spawnerNet != null ? spawnerNet.GetComponent<ServerCharacter>() : null;

                    if (m_CollisionCache[i].TryGetComponent(out IDamageable damageable))
                    {
                        damageable.ReceiveHP(spawnerObj, -m_ProjectileInfo.Damage);
                    }
                }

                if (m_IsDead)
                {
                    return; // don't keep examining collisions since we can't damage anybody else
                }
            }
        }
    }
    [ClientRpc]
    private void RecvHitEnemyClientRPC(ulong enemyId)
    {
        NetworkObject targetNetObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyId, out targetNetObject))
        {
            if (m_OnHitParticlePrefab)
            {
                // show an impact graphic
                Instantiate(m_OnHitParticlePrefab.gameObject, transform.position, transform.rotation);
            }
        }
    }
}
