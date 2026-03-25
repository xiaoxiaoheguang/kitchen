using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO KitchenObjectSO;

    public override void Interact(Player player)
    {

        if (!HasKitchenObject())
        {
            // There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                player.GetKitchenObj().SetKitchenObjectParent(this);
            }
            else
            {
                // Player not carrying anything
            }
        }
        else
        {
            // There is a KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // Player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObj().GetKitchenObjSO()))
                    {
                        KitchenObject.DestroyKitchenObj(GetKitchenObj());
                    }
                }
                else
                {
                    // Player is not carrying Plate but something else
                    if (GetKitchenObj().TryGetPlate(out plateKitchenObject))
                    {
                        // Counter is holding a Plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObj().GetKitchenObjSO()))
                        {
                            KitchenObject.DestroyKitchenObj(player.GetKitchenObj());
                        }
                    }
                }
            }
            else
            {
                // Player is not carrying anything
                GetKitchenObj().SetKitchenObjectParent(player);
            }
        }
    }
}
