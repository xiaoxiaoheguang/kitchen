using NUnit.Framework;
using System;
using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "Scriptable Objects/LevelSO")]
public class LevelSO : ScriptableObject
{
    public string levelSceneName;
    public int levelIndex;
    public string levelName;
    public Sprite levelImage;

}
