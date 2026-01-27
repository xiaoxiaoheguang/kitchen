using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{

    public static event EventHandler OnAnyObjectSethere;

    public static void ResetStaticData()
    {
        OnAnyObjectSethere = null;
    }

    [SerializeField] private Transform counterTopPoint;
    private KitchenObject KitchenObject;
    public virtual void Interact(Player player)
    {
        Debug.Log("Interact virtual");

    }
    public virtual void InteractAlternate(Player player)
    {
        Debug.Log("InteractAlternate virtual");
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.KitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnAnyObjectSethere?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject()
    {
        return KitchenObject;
    }
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return (KitchenObject != null);
    }
}
