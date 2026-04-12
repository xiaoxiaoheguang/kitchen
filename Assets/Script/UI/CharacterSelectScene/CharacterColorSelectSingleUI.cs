using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private Image selectImage;
    [SerializeField] private GameObject selectedGameObject;


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenMultiplayerGame.Instance.ChangePlayerColor(colorId);
        });
    }
    private void Start()
    {
        KitchenMultiplayerGame.Instance.OnPlayerDataNetworkListChanged += KitchenMultiplayerGame_OnPlayerDataNetworkListChanged;
        
        image.color = KitchenMultiplayerGame.Instance.GetPlayerColor(colorId);
        selectImage.color = image.color;

        UpdateIsSelected();
    }

    private void KitchenMultiplayerGame_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        if (KitchenMultiplayerGame.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        }
        else
        {
            selectedGameObject.SetActive(false);
        }
    }
}
