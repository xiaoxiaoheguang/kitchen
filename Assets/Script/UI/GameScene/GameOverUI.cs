using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;

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
            
            int totalScore = DeliveryManager.Instance.GetTotalScore();
            Debug.Log($"Total Score: {totalScore}");
            
            // 获取当前关卡的目标分数
            int targetScore = 0;
            List<LevelSO> levelSoList = GameModeManager.Instance?.LevelSoList;
            int currentLevelIndex = KitchenMultiplayerGame.Instance?.currentLevelIndex.Value ?? 0;
            if (levelSoList != null && levelSoList.Count > currentLevelIndex)
            {
                targetScore = levelSoList[currentLevelIndex].requiredScore;
            }
            
            // 设置文本
            totalScoreText.text = totalScore.ToString();
            targetScoreText.text = targetScore.ToString();
            
            // 判断胜利或失败
            bool isWin = totalScore >= targetScore;
            
            // 设置颜色
            Color textColor = isWin ? winColor : loseColor;
            totalScoreText.color = textColor;
            targetScoreText.color = textColor;
            
            // 根据胜利/失败状态控制下一关按钮的显示
            nextLevelButton.gameObject.SetActive(isWin);

            // 高级骚操作：在 Console 里直接显示颜色
            string hex = ColorUtility.ToHtmlStringRGBA(textColor);
            string totalHex = ColorUtility.ToHtmlStringRGBA(totalScoreText.color);
            string targetHex = ColorUtility.ToHtmlStringRGBA(targetScoreText.color);
            
            Debug.Log($"Is Win: <b>{isWin}</b>\n" +
                     $"Total Score: <color=#{totalHex}>{totalScore}</color>\n" +
                     $"Target Score: <color=#{targetHex}>{targetScore}</color>");


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
