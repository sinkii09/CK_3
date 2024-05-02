using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Chase Action")]
public class ChaseAction : Action
{
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        return ActionConclusion.Stop;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        return ActionConclusion.Continue;
    }
}
