using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterTypeEnum
{
    Warrior,
    Archer,
    Mage,
    Rogue,

    Skeleton_minion,
}

[CreateAssetMenu(menuName = "GameData/CharacterClass", order = 1)]
public class CharacterClass : ScriptableObject
{
    public CharacterTypeEnum CharacterType;

    public Action BaseAttack;

    public Action Skill1;

    public Action Skill2;

    public Action Skill3;

    public int BaseHP;

    public int BaseMana;

    public float Speed;

    public bool IsNpc;

    public float DetectRange;

    public string DisplayedName;
}
