using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterSelectVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter basecounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    void Start()
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged += Play_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }

    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnAnyPlayerSpawned;
            Player.LocalInstance.OnSelectedCounterChanged += Play_OnSelectedCounterChanged;
        }
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
