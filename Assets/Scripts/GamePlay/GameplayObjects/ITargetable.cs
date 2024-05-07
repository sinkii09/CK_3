using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable 
{
    bool IsNPC { get; }

    bool IsValidTarget { get; }
}
