using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ProjectileInfo
{
    public GameObject Prefab;
    public float Speed;
    public float Range;
    public int Damage;
    public int MaxVictims;
}
