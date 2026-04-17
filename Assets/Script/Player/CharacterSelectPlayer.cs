using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{

    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyText;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;


    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            PlayerData playerData = KitchenMultiplayerGame.Instance.GetPlayerDataFromPlayIndex(playerIndex);
            
            KitchenMultiplayerGame.Instance.KickPlayer(playerData.clientId);
        });
    }


    private void Start()
    {
        bool isSinglePlayer = GameModeManager.Instance != null && GameModeManager.Instance.IsSinglePlayerMode;
        
        if (!isSinglePlayer)
        {
            KitchenMultiplayerGame.Instance.OnPlayerDataNetworkListChanged += KitchenMultiplayerGame_OnPlayerDataNetworkListChanged;
        }
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void KitchenMultiplayerGame_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        bool isSinglePlayer = GameModeManager.Instance != null && GameModeManager.Instance.IsSinglePlayerMode;
        
        if (isSinglePlayer)
        {
            if (playerIndex == 0)
            {
                Show();
                readyText.SetActive(CharacterSelectReady.Instance.IsPlayerReady(0));
                playerVisual.SetPlayerColor(KitchenMultiplayerGame.Instance.GetPlayerColor(0));
                kickButton.gameObject.SetActive(false);
            }
            else
            {
                Hide();
            }
        }
        else
        {
            if (KitchenMultiplayerGame.Instance.IsPlayerIndexConnected(playerIndex))
            {
                Show();

                PlayerData playerData = KitchenMultiplayerGame.Instance.GetPlayerDataFromPlayIndex(playerIndex);
               
                readyText.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));

                playerVisual.SetPlayerColor(KitchenMultiplayerGame.Instance.GetPlayerColor(playerData.colorId));

                bool isHost = NetworkManager.Singleton.IsServer;
                bool isLocalPlayer = playerData.clientId == NetworkManager.Singleton.LocalClientId;
                kickButton.gameObject.SetActive(isHost && !isLocalPlayer);
            }
            else
            {
                Hide();
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        bool isSinglePlayer = GameModeManager.Instance != null && GameModeManager.Instance.IsSinglePlayerMode;
        
        if (!isSinglePlayer && KitchenMultiplayerGame.Instance != null)
        {
            KitchenMultiplayerGame.Instance.OnPlayerDataNetworkListChanged -= KitchenMultiplayerGame_OnPlayerDataNetworkListChanged;
        }
        if (CharacterSelectReady.Instance != null)
        {
            CharacterSelectReady.Instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
        }
    }
}
