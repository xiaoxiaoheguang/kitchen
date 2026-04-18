using UnityEngine;
using System.Collections.Generic;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public bool IsSinglePlayerMode { get; private set; }

    [SerializeField] private List<LevelSO> levelSoList;
    public List<LevelSO> LevelSoList => levelSoList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSinglePlayerMode(bool isSinglePlayer)
    {
        IsSinglePlayerMode = isSinglePlayer;
    }

    public void LoadSelectedLevel()
    {
        int currentIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value;
        if (levelSoList != null && levelSoList.Count > currentIndex)
        {
            Loader.LoadLevelNetwork(KitchenMultiplayerGame.Instance.currentLevelIndex.Value);
        }
    }

    public void NextLevel()
    {
        if (levelSoList == null || levelSoList.Count == 0) return;

        int newIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value + 1;
        if (newIndex >= levelSoList.Count)
        {
            newIndex = 0;
        }
        KitchenMultiplayerGame.Instance.currentLevelIndex.Value = newIndex;
    }

    public void PreviousLevel()
    {
        if (levelSoList == null || levelSoList.Count == 0) return;

        int newIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value - 1;
        if (newIndex < 0)
        {
            newIndex = levelSoList.Count - 1;
        }
        KitchenMultiplayerGame.Instance.currentLevelIndex.Value = newIndex;
    }
}
