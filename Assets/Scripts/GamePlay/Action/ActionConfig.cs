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
    CurveLaunchProjectile,
    Chase,
    Dash,
    Toss,
    Jump,
}


[Serializable]
public class ActionConfig
{
    public ActionLogic Logic;

    public int Damage;

    public int Amount;

    public float Range;

    public float DurationSeconds;

    public float ExecTimeSeconds;

    public float EffectDurationSeconds;

    public float ReuseTimeSeconds;

    public string AnimAnticipation;

    public string Anim;

    public string Anim2;

    public string ReactAnim;

    public string OtherAnimatorVariable;

    public float Radius;

    public BaseActionInput ActionInput;

    public bool ActionInterruptible;

    public List<Action> IsInterruptableBy;

    public BlockingModeType BlockingMode;

    public ProjectileInfo[] Projectiles;

    public WeaponInfo[] Spawns;

    public GameObject[] SpecialFX;

    public bool IsFriendly;

    public bool CanBeInterruptedBy(ActionID actionActionID)
    {
        foreach (var action in IsInterruptableBy)
        {
            if (action.ActionID == actionActionID)
            {
                return true;
            }
        }

        return false;
    }
}
