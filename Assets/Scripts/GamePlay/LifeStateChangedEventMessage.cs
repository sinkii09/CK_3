using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;

public struct LifeStateChangedEventMessage : INetworkSerializeByMemcpy
{
    public LifeState LifeState;
    public CharacterTypeEnum CharacterType;
}
