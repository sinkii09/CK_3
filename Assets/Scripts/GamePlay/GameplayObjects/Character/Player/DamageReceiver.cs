using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Unity.CK.GamePlay.GameplayObjects
{

    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<ServerCharacter, int> DamageReceived;

        public event Action<Collision> CollisionEntered;


        [SerializeField]
        NetworkLifeState m_NetworkLifeState;
        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {

            return m_NetworkLifeState.LifeState.Value == LifeState.Alive;
        }

        public void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, HP);
            }
        }
        void OnCollisionEnter(Collision other)
        {
            CollisionEntered?.Invoke(other);
        }
    }
}
