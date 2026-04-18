using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class LevelSelectUI : NetworkBehaviour
{
    [SerializeField] private Button leftButtons;
    [SerializeField] private Button rightButtons;
    [SerializeField] private Button returnButton;

    [SerializeField] private Button levelButton;
    [SerializeField] private Image levelImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI gameModeText;

    private void Awake()
    {
        levelButton.onClick.AddListener(OnLevelButtonClick);
        leftButtons.onClick.AddListener(OnLeftButtonClick);
        rightButtons.onClick.AddListener(OnRightButtonClick);
        returnButton.onClick.AddListener(OnReturnButtonClick);
    }

    private void Start()
    {
        UpdateButtonVisibility();
        UpdateLevelVisual();
        UpdateGameModeText();
    }

    public override void OnNetworkSpawn()
    {
        KitchenMultiplayerGame.Instance.currentLevelIndex.OnValueChanged += OnCurrentLevelIndexChanged;
        
        UpdateButtonVisibility();
        UpdateLevelVisual();
        UpdateGameModeText();
    }

    private void OnCurrentLevelIndexChanged(int previousValue, int newValue)
    {
        UpdateLevelVisual();
    }

    private void UpdateButtonVisibility()
    {
        bool shouldShowButtons = IsServer;
        
        leftButtons.gameObject.SetActive(shouldShowButtons);
        rightButtons.gameObject.SetActive(shouldShowButtons);
        levelButton.gameObject.SetActive(shouldShowButtons);
        returnButton.gameObject.SetActive(shouldShowButtons);
    }

    private void UpdateLevelVisual()
    {
        int currentIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value;
        List<LevelSO> levelSoList = GameModeManager.Instance?.LevelSoList;
        
        if (levelSoList != null && levelSoList.Count > currentIndex)
        {
            LevelSO levelSO = levelSoList[currentIndex];
            levelImage.sprite = levelSO.levelImage;
            if (levelText != null)
            {
                levelText.text = levelSO.levelName;
            }
        }
    }

    private void UpdateGameModeText()
    {
        if (gameModeText != null)
        {
            bool isSinglePlayer = GameModeManager.Instance != null && GameModeManager.Instance.IsSinglePlayerMode;
            gameModeText.text = isSinglePlayer ? "单人模式" : "多人模式";
        }
    }

    private void OnRightButtonClick()
    {
        if (!IsServer) return;

        GameModeManager.Instance?.NextLevel();
        UpdateLevelVisual();
    }

    private void OnLeftButtonClick()
    {
        if (!IsServer) return;

        GameModeManager.Instance?.PreviousLevel();
        UpdateLevelVisual();
    }

    private void OnLevelButtonClick()
    {
        if (!IsServer) return;

        GameModeManager.Instance?.LoadSelectedLevel();
    }

    private void OnReturnButtonClick()
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    public override void OnNetworkDespawn()
    {
        if (KitchenMultiplayerGame.Instance != null && KitchenMultiplayerGame.Instance.currentLevelIndex != null)
        {
            KitchenMultiplayerGame.Instance.currentLevelIndex.OnValueChanged -= OnCurrentLevelIndexChanged;
        }
    }


}
