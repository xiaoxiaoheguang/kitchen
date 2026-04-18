using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deliveryAmount;
    [SerializeField] private TextMeshProUGUI targetAmount;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button nextLevelButton;

    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
     private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        nextLevelButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                GameModeManager.Instance?.NextLevel();
                GameModeManager.Instance?.LoadSelectedLevel();
            }
        });
    }


    private void Start()
    {
        GameManager.Instance.OnStateChanged += Instance_OnStateChange;
        Hide();
    }

    private void Instance_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            Show();
            
            int deliveryCount = DeliveryManager.Instance.GetSuccessRecipeSOCount();
            
            // 获取当前关卡的目标分数
            int targetCount = 0;
            List<LevelSO> levelSoList = GameModeManager.Instance?.LevelSoList;
            int currentLevelIndex = KitchenMultiplayerGame.Instance?.currentLevelIndex.Value ?? 0;
            if (levelSoList != null && levelSoList.Count > currentLevelIndex)
            {
                targetCount = levelSoList[currentLevelIndex].requiredScore;
            }
            
            // 设置文本
            deliveryAmount.text = deliveryCount.ToString();
            targetAmount.text = targetCount.ToString();
            
            // 判断胜利或失败
            bool isWin = deliveryCount >= targetCount;
            
            // 设置颜色
            Color textColor = isWin ? winColor : loseColor;
            deliveryAmount.color = textColor;
            targetAmount.color = textColor;
            
            // 根据胜利/失败状态控制下一关按钮的显示
            nextLevelButton.gameObject.SetActive(isWin);

            // 高级骚操作：在 Console 里直接显示颜色
            string hex = ColorUtility.ToHtmlStringRGBA(textColor);
            string deliveryHex = ColorUtility.ToHtmlStringRGBA(deliveryAmount.color);
            string targetHex = ColorUtility.ToHtmlStringRGBA(targetAmount.color);
            
            Debug.Log($"Is Win: <b>{isWin}</b>\n" +
                     $"Text Color: <color=#{hex}>■■■■ #{hex}</color>\n" +
                     $"deliveryAmount.color: <color=#{deliveryHex}>■■■■ #{deliveryHex}</color>\n" +
                     $"targetAmount.color: <color=#{targetHex}>■■■■ #{targetHex}</color>");


        }
        else
        {
            Hide();
        }
    }


    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }

}
