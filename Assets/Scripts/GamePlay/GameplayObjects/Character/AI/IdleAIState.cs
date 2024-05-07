using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAIState : AIState
{
    private AIBrain m_Brain;

    public IdleAIState(AIBrain brain)
    {
        m_Brain = brain;
    }
    public override void Initialize()
    {

    }

    public override bool IsEligible()
    {
        return m_Brain.GetHatedEnemies().Count == 0;
    }

    public override void Update()
    {
        DetectFoes();
    }
    protected void DetectFoes()
    {
        float detectionRange = m_Brain.DetectRange;
        float detectionRangeSqr = detectionRange * detectionRange;
        Vector3 position = m_Brain.GetMyServerCharacter().physicsWrapper.Transform.position;
        foreach (var character in PlayerServerCharacter.GetPlayerServerCharacters())
        {
            if (m_Brain.IsAppropriateFoe(character) && (character.physicsWrapper.Transform.position - position).sqrMagnitude <= detectionRangeSqr)
            {
                m_Brain.Hate(character);
            }
        }
    }
}
