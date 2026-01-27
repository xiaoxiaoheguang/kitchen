using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        GameManager.Instance.OnStateChange += Instance_OnStateChange;
        Hide();
    }
    private void Update()
    {
        countdownText.text = Mathf.Ceil(GameManager.Instance.GetCountdownTimer()).ToString();
    }

    private void Instance_OnStateChange(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameCountdownActive())
        {
            Show();
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
