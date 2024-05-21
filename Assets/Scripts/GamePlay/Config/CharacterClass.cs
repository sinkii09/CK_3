using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterTypeEnum
{
    Babarian,
    Archer,
    Mage,
    Knight,

    Skeleton_mage,
    Skeleton_Rouge,
    Skeleton_Warrior,
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
