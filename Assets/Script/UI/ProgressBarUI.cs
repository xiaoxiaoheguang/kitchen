using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{ 
    [SerializeField] private GameObject hasProgressGameObject;
     private IHasProgress IhasProgress;
    [SerializeField] private Image barImage;

    private void Start()
    {
        IhasProgress= hasProgressGameObject.GetComponent<IHasProgress>();
        if(IhasProgress == null)
        {
            Debug.LogError("gameObject" + hasProgressGameObject + "doea no has a component that implements IHasProgress");
        }
        IhasProgress.OnProgressChange += IHasProgress_OnProgressChange;
        barImage.fillAmount = 0f;

        Hide();
    }

    private void IHasProgress_OnProgressChange(object sender, IHasProgress.OnProgressChangeEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;
        if (e.progressNormalized == 0f || e.progressNormalized == 1f)
        {
            Hide();
        }
        else
        {
            Show();
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
