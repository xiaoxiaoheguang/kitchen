using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deliveryAmount;


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
        deliveryAmount.text = DeliveryManager.Instance.GetSuccessRecipeSOCount().ToString();
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
