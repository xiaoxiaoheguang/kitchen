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

    [SerializeField] private List<LevelSO> levelSoList;

    private int currentLevelIndex = 0;

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
    }

    public override void OnNetworkSpawn()
    {
        KitchenMultiplayerGame.Instance.currentLevelIndex.OnValueChanged += OnCurrentLevelIndexChanged;
        
        UpdateButtonVisibility();
        UpdateLevelVisual();
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

    private void OnRightButtonClick()
    {
        if (!IsServer) return;

        int newIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value + 1;
        if (newIndex >= levelSoList.Count)
        {
            newIndex = 0;
        }
        KitchenMultiplayerGame.Instance.currentLevelIndex.Value = newIndex;
        
        UpdateLevelVisual();
    }

    private void OnLeftButtonClick()
    {
        if (!IsServer) return;

        int newIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value - 1;
        if (newIndex < 0)
        {
            newIndex = levelSoList.Count - 1;
        }
        KitchenMultiplayerGame.Instance.currentLevelIndex.Value = newIndex;
        
        UpdateLevelVisual();
    }

    private void OnLevelButtonClick()
    {
        if (!IsServer) return;

        int currentIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value;
        if (levelSoList != null && levelSoList.Count > currentIndex)
        {
            Loader.LoadLevelNetwork(KitchenMultiplayerGame.Instance.currentLevelIndex.Value);
        }
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
