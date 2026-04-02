using System;
using UnityEngine;

public class PauseMultiplayUI : MonoBehaviour
{

        private void Start()
        {
            GameManager.Instance.OnMultiplayGamePause += GameManager_OnMultiplayGamePause;
            GameManager.Instance.OnMultiplayGameResume += GameManager_OnMultiplayGameResume;
    
            Hide();
    }

    private void GameManager_OnMultiplayGameResume(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnMultiplayGamePause(object sender, EventArgs e)
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
}
