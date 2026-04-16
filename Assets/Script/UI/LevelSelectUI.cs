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

    [SerializeField] private Button levelButton;
    [SerializeField] private Image levelImage;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private List<LevelSO> levelSoList;

    private void Awake()
    {
        levelButton.onClick.AddListener(OnLevelButtonClick);
        leftButtons.onClick.AddListener(OnLeftButtonClick);
        rightButtons.onClick.AddListener(OnRightButtonClick);
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
        bool isHost = IsServer;
        leftButtons.gameObject.SetActive(isHost);
        rightButtons.gameObject.SetActive(isHost);
        levelButton.gameObject.SetActive(isHost);
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
    }

    private void OnLevelButtonClick()
    {
        if (!IsServer) return;

        int currentIndex = KitchenMultiplayerGame.Instance.currentLevelIndex.Value;
        if (levelSoList != null && levelSoList.Count > currentIndex)
        {
            LevelSO levelSO = levelSoList[currentIndex];
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (KitchenMultiplayerGame.Instance != null && KitchenMultiplayerGame.Instance.currentLevelIndex != null)
        {
            KitchenMultiplayerGame.Instance.currentLevelIndex.OnValueChanged -= OnCurrentLevelIndexChanged;
        }
    }


}
