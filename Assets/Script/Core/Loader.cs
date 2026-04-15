using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader {
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene,
        CharacterSelectScene,
        LobbyScene,
        LevelSelectScene,
    }

    public enum LevelScene
    {
        Level_1_Scene,
        Level_2_Scene,
        Level_3_Scene,
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
    public static void LoadLevelNetwork(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void LoadLevelNetwork(int levelIndex)
    {
        string sceneName = ((LevelScene)levelIndex).ToString();
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public static void LoadCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());

    }
}
