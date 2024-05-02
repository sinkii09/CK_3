using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GamePlay/Actions/Target Action")]
public class TargetAction : Action
{
    public override bool OnStart(ServerCharacter serverCharacter)
    {
        return true;
    }

    public override bool OnUpdate(ServerCharacter serverCharacter)
    {
        return true;
    }
}
