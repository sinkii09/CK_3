using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ActionLogic
{
    Melee,
    RangedTargeted,
    LaunchProjectile,
    AoE,
    Target,
    ChargedLaunchProjectile,
}


[Serializable]
public class ActionConfig
{
    public ActionLogic Logic;

    public int Amount;

    public int ManaCost;

    public float Range;

    public float DurationSeconds;

    public float ExecTimeSeconds;

    public string AnimAnticipation;

    public string Anim;

    public string Anim2;

    public string ReactAnim;

    public float Radius;

    public BaseActionInput ActionInput;

    public bool ActionInterruptible;

    public ProjectileInfo[] Projectiles;

    public GameObject[] Spawns;

    public bool IsFriendly;
}
