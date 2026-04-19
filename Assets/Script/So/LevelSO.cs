using NUnit.Framework;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "Scriptable Objects/LevelSO")]
public class LevelSO : ScriptableObject
{
    public string levelSceneName;
    public int levelIndex;
    public string levelName;
    public Sprite levelImage;
    public int levelTime;
public RecipeListSO levelRecipeListSO;


    public int requiredScore;

}
