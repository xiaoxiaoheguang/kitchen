using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenMultiplayerGame : NetworkBehaviour
{
    [SerializeField] private int MAX_PLAYERCOUNT = 4;
    public static KitchenMultiplayerGame Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;


    public KitchenObjectListSO kitchenObjectListSO;

    void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();

    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started!";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYERCOUNT)
        {
            response.Approved = false;
            response.Reason = "Game is full!";
            return;
        }
        response.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientId) =>
        {
            OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        };
        NetworkManager.Singleton.StartClient();

    }

    public void SpawKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawKitchenObjectServerRpc(GetKitchenObjSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjParentNetworkObjRef)
    {
        // ����ServerRpcֻ�ܴ��ݻ������ͺ����л����ͣ��������Ǵ���KitchenObjectSO����������ȡ��Ӧ��KitchenObjectSO����
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
