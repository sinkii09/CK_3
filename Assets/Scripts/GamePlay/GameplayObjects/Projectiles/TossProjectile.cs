using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TossProjectile : NetworkBehaviour
{
    [SerializeField] int m_DamagePoints;

    [SerializeField] float m_HitRadius = 5f;

    [SerializeField] float m_KnockbackSpeed;

    [SerializeField]
    float m_KnockbackDuration;

    [SerializeField]
    LayerMask m_LayerMask;

    [SerializeField]
    float m_DetonateAfterSeconds = 3f;

    [SerializeField]
    float m_DestroyAfterSeconds = 4f;

    float m_DetonateTimer;

    float m_DestroyTimer;

    bool m_Detonated;
    
    bool m_Started;

    ServerCharacter m_Character;

    public UnityEvent detonatedCallback;

    [SerializeField]
    Transform m_TossedExplosionVisualTransform;

    [SerializeField]
    GameObject m_TossedObjectGraphics;

    const float k_DisplayHeight = 0.1f;

    readonly Quaternion k_TossAttackRadiusDisplayRotation = Quaternion.Euler(90f, 0f, 0f);
    public void Initialize(ServerCharacter serverCharacter)
    {
        m_Character = serverCharacter;
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            m_Started = true;

            m_Detonated = false;

            m_DetonateTimer = Time.fixedTime + m_DetonateAfterSeconds;
            m_DestroyTimer = Time.fixedTime + m_DestroyAfterSeconds;
        }
        if(IsClient)
        {
            m_TossedExplosionVisualTransform.gameObject.SetActive(true);
            m_TossedObjectGraphics.SetActive(true);
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            m_Started = false;
            m_Detonated = false;
        }
        if (IsClient)
        {
            m_TossedExplosionVisualTransform.gameObject.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (!m_Started)
            {
                return;
            }
            if (!m_Detonated && m_DetonateTimer < Time.fixedTime)
            {
                Detonate();
            }
            if (m_Detonated && m_DestroyTimer < Time.fixedTime)
            {
                var networkObject = gameObject.GetComponent<NetworkObject>();
                networkObject.Despawn();
            }
        }
    }

    private void LateUpdate()
    {
        if (IsClient)
        {
            var tossedItemPosition = transform.position;
            m_TossedExplosionVisualTransform.SetPositionAndRotation(
                new Vector3(tossedItemPosition.x, k_DisplayHeight, tossedItemPosition.z),
                k_TossAttackRadiusDisplayRotation);
        }
    }
    void Detonate()
    {
        Collider[] results = new Collider[16];
        var hits = Physics.OverlapSphereNonAlloc(transform.position, m_HitRadius, results, m_LayerMask);
        for(int i = 0; i < hits;  i++)
        {
            if (results[i].TryGetComponent(out IDamageable damageable))
            {
                damageable.ReceiveHP(m_Character, -m_DamagePoints);
                var serverCharacter = results[i].gameObject.GetComponentInParent<ServerCharacter>();
                if (serverCharacter)
                {
                    serverCharacter.Movement.StartKnockback(transform.position, m_KnockbackSpeed, m_KnockbackDuration);
                }
            }
        }

        DetonateClientRpc();

        m_Detonated = true;
    }
    [ClientRpc]
    void DetonateClientRpc()
    {
        detonatedCallback?.Invoke();
    }
}
