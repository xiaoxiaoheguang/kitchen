using Unity.Netcode;
using UnityEngine;

public class KitchenMultiplayerGame : NetworkBehaviour
{
    public static KitchenMultiplayerGame Instance { get; private set; }

    public KitchenObjectListSO kitchenObjectListSO;

    void Awake()
    {
        Instance = this;
    }


    public void SpawKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership =false)]
    public void SpawKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjParentNetworkObjRef)
    {
        // 由于ServerRpc只能传递基本类型和序列化类型，所以我们传递KitchenObjectSO的索引来获取对应的KitchenObjectSO对象。
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObjParentNetworkObjRef.TryGet(out NetworkObject kitchenObjParentNetworkObj);
        IKitchenObjectParent kitchenObjectParent = kitchenObjParentNetworkObj.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    private int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    private KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }
}
