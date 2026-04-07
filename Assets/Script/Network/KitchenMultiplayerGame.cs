using System;
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
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();

    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(GameManager.Instance.IsWaitingToStart())
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            response.Approved = false;
        }

    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();

    }

    public void SpawKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawKitchenObjectServerRpc(GetKitchenObjSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership =false)]
    public void SpawKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjParentNetworkObjRef)
    {
        // 由于ServerRpc只能传递基本类型和序列化类型，所以我们传递KitchenObjectSO的索引来获取对应的KitchenObjectSO对象。
        KitchenObjectSO kitchenObjectSO = GetKitchenObjSOFromIndex(kitchenObjectSOIndex);

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

    public KitchenObjectSO GetKitchenObjSOFromIndex(int kitchenObjectSOIndex)
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
