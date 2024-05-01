using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.CK.GamePlay.Configuration
{
    [CreateAssetMenu]
    [Serializable]
    public sealed class Avatar : ScriptableObject
    {
        public CharacterClass characterClass;

        public GameObject Graphics;

        public GameObject GraphicsCharacterSelect;

        public Sprite Potrait;
    }
}

