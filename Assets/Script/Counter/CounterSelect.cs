using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterSelect : MonoBehaviour
{
    [SerializeField] private BaseCounter basecounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    void Start()
    {
        //Player.instance.OnSelectedCounterChanged += Play_OnSelectedCounterChanged;
    }

    private void Play_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter != basecounter)
        {
            Hide();

        }
        else
        {
             Show();
        }
    }
    private void Show()
    {
        foreach (GameObject go in visualGameObjectArray)
        {
            go.SetActive(true);
        }
    }
    private void Hide()
    {
        foreach (GameObject go in visualGameObjectArray)
        {
            go.SetActive(false);
        }
    }
}
