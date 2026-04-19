using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    private void OnEnable()
    {
        // 确保 GameManager 存在后再更新
        if (GameManager.Instance != null)
        {
            UpdateTimerDisplay();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        // 使用 NetworkVariable 的值来确保同步
        timerImage.fillAmount = GameManager.Instance.GetGameplayTimerNormalized();
    }
}
