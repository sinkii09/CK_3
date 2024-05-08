using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CurvePhysicsProjectile : NetworkBehaviour
{
    public UnityEvent detonatedCallback;
    
    [SerializeField] private AnimationCurve arcYAnimationCurve;

    [SerializeField]
    Transform m_Visualization;

    const float k_LerpTime = 0.1f;
    PositionLerper m_PositionLerper;

    private Vector3 targetPosition;
    private float totalDistane;
    private Vector3 positionXZ;
    private float m_HitRadius;
    private float moveSpeed;
    int m_DamagePoints;


    const int k_MaxCollisions = 16;
    Collider[] m_CollisionCache = new Collider[k_MaxCollisions];

    Vector3 moveDir;

    bool m_Detonated;
    bool m_Started;

    int m_CollisionMask;
    int m_NpcLayer;
    public void Initialize(Vector3 targetPosition, in ProjectileInfo projectileInfo)
    {
        this.targetPosition = targetPosition;
        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistane = Vector3.Distance(positionXZ, targetPosition);
        m_HitRadius = projectileInfo.Range;
        moveSpeed = projectileInfo.Speed;
        m_DamagePoints = projectileInfo.Damage;
    }

    private void Update()
    {
        if (IsClient)
        {
            if (IsHost)
            {
                m_Visualization.position = m_PositionLerper.LerpPosition(m_Visualization.position,
                    transform.position);
            }
            else
            {
                m_Visualization.position = transform.position;
            }
        }

        moveDir = (targetPosition - positionXZ).normalized;

        float reachedTargetDistance = .2f;
        if (Vector3.Distance(positionXZ, targetPosition) <= reachedTargetDistance)
        {
            Detonate();
        }
    }
    private void FixedUpdate()
    {
        if (!m_Started || !IsServer || m_Detonated)
        {
            return;
        }
        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistane;
        float maxHeight = totalDistane / 4f;

        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

    }
    void Detonate()
    {
        var hits = Physics.OverlapSphereNonAlloc(transform.position, m_HitRadius, m_CollisionCache);
        
        for(int i = 0; i < hits; i++)
        {
            if (m_CollisionCache[i].gameObject.TryGetComponent(out IDamageable damageReceiver))
            {
                var serverCharacter = m_CollisionCache[i].gameObject.GetComponentInParent<ServerCharacter>();
                if (serverCharacter && serverCharacter.IsNPC)
                {
                    damageReceiver.ReceiveHP(null, -m_DamagePoints);
                }
            }
        }
        DetonateClientRpc();
        m_Detonated = true;
        var networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_Started = true;
            m_Detonated = false;
            m_CollisionMask = LayerMask.GetMask(new[] { "Ground" });
        }
        if(IsClient)
        {
            m_Visualization.gameObject.SetActive(true);
            m_Visualization.parent = null;
            m_PositionLerper = new PositionLerper(transform.position, k_LerpTime);
            m_Visualization.transform.rotation = transform.rotation;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            m_Started = false;
            m_Detonated = false;
        }
        if(IsClient)
        {
            m_Visualization.gameObject.SetActive(false);
            m_Visualization.parent = transform;
        }
    }

    [ClientRpc]
    void DetonateClientRpc()
    {
        detonatedCallback?.Invoke();
    }
}
