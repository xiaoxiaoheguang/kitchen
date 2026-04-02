using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] Button resumeButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button optionButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.PauseGame();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        optionButton.onClick.AddListener(() =>
        {
            Hide();
            OptionUI.Instance.Show(Show);
        });
    }
    private void Start()
    {
        GameManager.Instance.OnLocalGamePause += GameManager_OnLocalPauseGame;
        GameManager.Instance.OnLocalGameResume += GameManager_OnLocalResumeGame;

        Hide();
    }

    private void GameManager_OnLocalResumeGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLocalPauseGame(object sender, System.EventArgs e)
    {
       Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        
    }
    private void Show()
    {
        gameObject.SetActive(true);
        resumeButton.Select(); 
    }

}
