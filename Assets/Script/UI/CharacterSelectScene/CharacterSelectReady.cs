using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;

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
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

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
            Loader.LoadLevelNetwork(KitchenMultiplayerGame.Instance.currentLevelIndex.Value);

        }

    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyMap[clientId] = true;

        OnReadyChanged?.Invoke(this, new EventArgs());
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyMap.ContainsKey(clientId) && playerReadyMap[clientId];
    }
}
