using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public static OptionUI Instance { get; private set; }



    [SerializeField] Button soundEffectButton;
    [SerializeField] Button musicButton;
    [SerializeField] Button closeButton;

    [SerializeField] TextMeshProUGUI soundEffectText;
    [SerializeField] TextMeshProUGUI musicText;


    [SerializeField] Button moveUpButton;
    [SerializeField] Button moveDownButton;
    [SerializeField] Button moveLeftButton;
    [SerializeField] Button moveRightButton;
    [SerializeField] Button interactButton;
    [SerializeField] Button interactAltButton;
    [SerializeField] Button pauseButton;

    [SerializeField] TextMeshProUGUI moveUpText;
    [SerializeField] TextMeshProUGUI moveDownText;
    [SerializeField] TextMeshProUGUI moveLeftText;
    [SerializeField] TextMeshProUGUI moveRightText;
    [SerializeField] TextMeshProUGUI interactText;
    [SerializeField] TextMeshProUGUI interactAltText;
    [SerializeField] TextMeshProUGUI pauseText;

    private Action onCloseButtonAction;

    [SerializeField] Transform pressRebindTransform;
    private void Awake()
    {
        Instance = this;


        soundEffectButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        closeButton.onClick.AddListener(() =>
        {
            Hide();
            onCloseButtonAction();
        });

        moveUpButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_UP);
        });

        moveDownButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_DOWN);
        }); 

        moveLeftButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_LEFT);
        }); 

        moveRightButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Move_RIGHT);
        }); 

        interactButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Interact);
        }); 

        interactAltButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.InteractAlt);
        }); 

        pauseButton.onClick.AddListener(() =>
        {
            RebindBinding(GameInput.Binding.Pause);
        });

    }
    private void Start()
    {
        GameManager.Instance.OnLocalGameResume += OptionUI_OnPauseGame;
        UpdateVisual();

        Hide();
        HidePressRebind();
    }

    private void OptionUI_OnPauseGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void UpdateVisual()
    {
        soundEffectText.text = "SoundEffect:" + Mathf.Round(SoundManager.Instance.GetVolume() * 10f).ToString();
        musicText.text = "Music:" + Mathf.Round(MusicManager.Instance.GetVolume() * 10f).ToString();

        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_UP);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_DOWN);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_LEFT);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_RIGHT);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAltText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlt);
        pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show(Action onCloseButtonAction)
    {
        this.onCloseButtonAction= onCloseButtonAction;

        gameObject.SetActive(true);
        soundEffectButton.Select();
    }

    private void HidePressRebind()
    {
        pressRebindTransform.gameObject.SetActive(false);
    }
    private void ShowPressRebind()
    {
        pressRebindTransform.gameObject.SetActive(true);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressRebind();
        GameInput.Instance.RebindBinding(binding, () =>
        {
            HidePressRebind();
            UpdateVisual();
        });
    }
}
