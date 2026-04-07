using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Start Host");
            KitchenMultiplayerGame.Instance.StartHost();
            Hide();
        });
        
        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Start Client");
            KitchenMultiplayerGame.Instance.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
