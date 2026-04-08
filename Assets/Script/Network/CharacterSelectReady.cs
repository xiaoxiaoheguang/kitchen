using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    private Dictionary<ulong, bool> playerReadyMap;

    private void Awake()
    {
        Instance = this;
        playerReadyMap = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyMap[serverRpcParams.Receive.SenderClientId] = true;

        bool isAllClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyMap.ContainsKey(clientId) || !playerReadyMap[clientId])
            {
                isAllClientsReady = false;
                break;
            }
        }

        if (isAllClientsReady)
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }

    }
}
