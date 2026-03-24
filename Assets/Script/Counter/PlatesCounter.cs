using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawn;
    public event EventHandler OnPlateRemove;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4f;
    private int plateSpawnAmount;
    private int plateSpawnAmountMax = 4;
    private void Update()
    {
        if (!IsServer) return;

        spawnPlateTimer += Time.deltaTime;

        if (spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;

            if (GameManager.Instance.IsGamePlaying() && plateSpawnAmount < plateSpawnAmountMax)
            {
                SpawnPlateServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        plateSpawnAmount++;
        OnPlateSpawn?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            if (plateSpawnAmount > 0)
            {
                KitchenObject.SpawKitchenObject(plateKitchenObjectSO, player);

                InteractLogicServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        plateSpawnAmount--;
        OnPlateRemove?.Invoke(this, EventArgs.Empty);
    }
}
