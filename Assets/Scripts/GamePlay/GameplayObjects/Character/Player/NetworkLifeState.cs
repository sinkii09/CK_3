using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;

namespace Unity.CK.GamePlay.GameplayObjects
{
    public enum LifeState
    {
        Alive,
        Fainted,
        Dead,
    }
    public class NetworkLifeState : NetworkBehaviour
    {
        [SerializeField]
        NetworkVariable<LifeState> m_LifeState = new NetworkVariable<LifeState>(GameplayObjects.LifeState.Alive);

        public NetworkVariable<LifeState> LifeState => m_LifeState;
    }
}
