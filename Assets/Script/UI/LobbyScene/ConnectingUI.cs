using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        KitchenMultiplayerGame.Instance.OnTryingToJoinGame += KitchenMultiplayerGame_OnTryingToJoinGame;
        KitchenMultiplayerGame.Instance.OnFailedToJoinGame += KitchenMultiplayerGame_OnFailedToJoinGame;

        Hide();
    }

    private void KitchenMultiplayerGame_OnFailedToJoinGame(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenMultiplayerGame_OnTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
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
        KitchenMultiplayerGame.Instance.OnTryingToJoinGame -= KitchenMultiplayerGame_OnTryingToJoinGame;
        KitchenMultiplayerGame.Instance.OnFailedToJoinGame -= KitchenMultiplayerGame_OnFailedToJoinGame;
    }
}
