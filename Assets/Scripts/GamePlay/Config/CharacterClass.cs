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
    Boss
}

[CreateAssetMenu(menuName = "GameData/CharacterClass", order = 1)]
public class CharacterClass : ScriptableObject
{
    public CharacterTypeEnum CharacterType;

    public Action BaseAttack;

    public int BaseHP;

    public float Speed;

    public bool IsNpc;

    public string DisplayedName;
}
