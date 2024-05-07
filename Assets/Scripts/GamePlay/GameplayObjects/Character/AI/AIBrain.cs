using System;
using System.Collections;
using System.Collections.Generic;
using Unity.CK.GamePlay.GameplayObjects;
using UnityEngine;

public class AIBrain 
{
    private enum AIStateType
    {
        IDLE,
        ATTACK,
    }

    static readonly AIStateType[] k_AIStates = (AIStateType[])Enum.GetValues(typeof(AIStateType));

    private ServerCharacter m_ServerCharacter;
    private ServerActionPlayer m_ServerActionPlayer;
    private AIStateType m_CurrentState;
    private Dictionary<AIStateType, AIState> m_Logics;
    private List<ServerCharacter> m_HatedEnemies;

    private float m_DetectRangeOverride = -1;

    public AIBrain(ServerCharacter me, ServerActionPlayer myServerActionPlayer)
    {
        m_ServerCharacter = me;
        m_ServerActionPlayer = myServerActionPlayer;

        m_Logics = new Dictionary<AIStateType, AIState>
        {
            [AIStateType.IDLE] = new IdleAIState(this),
            
            [AIStateType.ATTACK] = new AttackAIState(this, m_ServerActionPlayer),
        };
        m_HatedEnemies = new List<ServerCharacter>();
        m_CurrentState = AIStateType.IDLE;
    }

    public void Update()
    {
        AIStateType newState = FindBestEligibleAIState();
        if (m_CurrentState != newState)
        {
            m_Logics[newState].Initialize();
        }
        m_CurrentState = newState;
        m_Logics[m_CurrentState].Update();
    }

    public void ReceiveHP(ServerCharacter inflicter, int amount)
    {
        if (inflicter != null && amount < 0)
        {
            Hate(inflicter);
        }
    }

    private AIStateType FindBestEligibleAIState()
    {
        foreach (AIStateType aiStateType in k_AIStates)
        {
            if (m_Logics[aiStateType].IsEligible())
            {
                return aiStateType;
            }
        }
        return AIStateType.IDLE;
    }
    public bool IsAppropriateFoe(ServerCharacter potentialFoe)
    {
        if (potentialFoe == null ||
            potentialFoe.IsNPC ||
            potentialFoe.LifeState != LifeState.Alive)
        {
            return false;
        }
        return true;
    }
    public void Hate(ServerCharacter character)
    {
        if (!m_HatedEnemies.Contains(character))
        {
            m_HatedEnemies.Add(character);
        }
    }
    public List<ServerCharacter> GetHatedEnemies()
    {
        for (int i = m_HatedEnemies.Count - 1; i >= 0; i--)
        {
            if (!IsAppropriateFoe(m_HatedEnemies[i]))
            {
                m_HatedEnemies.RemoveAt(i);
            }
        }
        return m_HatedEnemies;
    }
    public ServerCharacter GetMyServerCharacter()
    {
        return m_ServerCharacter;
    }
    public CharacterClass CharacterData
    {
        get
        {
            return GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
        }
    }
    public float DetectRange
    {
        get
        {
            return (m_DetectRangeOverride == -1) ? CharacterData.DetectRange : m_DetectRangeOverride;
        }

        set
        {
            m_DetectRangeOverride = value;
        }
    }
}
