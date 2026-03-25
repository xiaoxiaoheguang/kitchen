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
        SpawKitchenObjectServerRpc(GetKitchenObjSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
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

    public int GetKitchenObjSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }


    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjNetworkObjRef)
    {
        kitchenObjNetworkObjRef.TryGet(out NetworkObject kitchenObjNetworkObj);
        KitchenObject kitchenObject = kitchenObjNetworkObj.GetComponent<KitchenObject>();

        ClearKitchenObjOnParentClientRpc(kitchenObjNetworkObjRef);
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjOnParentClientRpc(NetworkObjectReference kitchenObjNetworkObjRef)
    {
        kitchenObjNetworkObjRef.TryGet(out NetworkObject kitchenObjNetworkObj);
        KitchenObject kitchenObject = kitchenObjNetworkObj.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectParent();
    }
}
