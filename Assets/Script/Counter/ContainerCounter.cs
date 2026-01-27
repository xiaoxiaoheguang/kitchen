using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;
    [SerializeField] private KitchenObjectSO ContainKitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            KitchenObject.SpawKitchenObject(ContainKitchenObjectSO, player);
            //Transform kitchenObjectTransform = Instantiate(KitchenObjectSO.prefab);
            //kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);

            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }
}
