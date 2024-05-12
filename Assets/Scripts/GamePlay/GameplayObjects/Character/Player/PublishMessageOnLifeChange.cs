using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class PublishMessageOnLifeChange : NetworkBehaviour
{
    NetworkLifeState m_NetworkLifeState;
    ServerCharacter m_ServerCharacter;


    [Inject]
    IPublisher<LifeStateChangedEventMessage> m_Publisher;

    private void Awake()
    {
        m_NetworkLifeState = GetComponent<NetworkLifeState>();
        m_ServerCharacter = GetComponent<ServerCharacter>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
        
            var gameState = FindObjectOfType<ServerGamePlayState>();
            if(gameState != null)
            {
                gameState.Container.Inject(this);
            }
        }
    }

    private void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
    {
        m_Publisher.Publish(new LifeStateChangedEventMessage()
        {
            CharacterType = m_ServerCharacter.CharacterClass.CharacterType,
            LifeState = newValue,
        }) ;
    }
}
