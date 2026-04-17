using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button singleGameStartButton;

    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;


    private void Awake()
    {
        singleGameStartButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            GameModeManager.Instance.SetSinglePlayerMode(true);
            KitchenMultiplayerGame.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });

        createGameButton.onClick.AddListener(() =>
        {
            GameModeManager.Instance.SetSinglePlayerMode(false);
            KitchenMultiplayerGame.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });
        
        joinGameButton.onClick.AddListener(() =>
        {
            GameModeManager.Instance.SetSinglePlayerMode(false);
            KitchenMultiplayerGame.Instance.StartClient();
        });
    }

}
