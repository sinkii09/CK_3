using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    public abstract bool IsEligible();

    public abstract void Initialize();

    public abstract void Update();
}
