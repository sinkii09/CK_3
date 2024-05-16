using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/NameGeneration", order = 2)]
public class NameGenerationData : ScriptableObject
{
    public string[] FirstWordList;
    public string[] SecondWordList;

    public string GenerateName()
    {
        var firstWord = FirstWordList[Random.Range(0, FirstWordList.Length - 1)];
        var seconWord = SecondWordList[Random.Range(0, SecondWordList.Length - 1)];

        return firstWord + " " +seconWord;
    }
}
