using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlateKitchenObject : KitchenObject
{

    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> vaildKitchenObjectSOList;
    private List<KitchenObjectSO> kitchenObjectSOList;
    protected override void Awake()
    {
        base.Awake(); 
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!vaildKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        else
        {
            AddIngredientServerRpc(
                KitchenMultiplayerGame.Instance.GetKitchenObjSOIndex(kitchenObjectSO)
                );
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kichenObjectSOIndex)
    {
       AddIngredientClientRpc(kichenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kichenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenMultiplayerGame.Instance.GetKitchenObjSOFromIndex(kichenObjectSOIndex);

        kitchenObjectSOList.Add(kitchenObjectSO);

        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            kitchenObjectSO = kitchenObjectSO,
        });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}