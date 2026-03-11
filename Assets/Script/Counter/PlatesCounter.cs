using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnplateSpawn;
    public event EventHandler OnplateRemove;
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    
    private float spawnPlateTimer;
    private float spawnPlateTimerMax=4f;
    private int plateSpawnAmount;
    private int plateSpawnAmountMax=4;
    private void Update()
    {
        spawnPlateTimer += Time.deltaTime;

        if(spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;

            if (GameManager.Instance.IsGamePlaying() && plateSpawnAmount < plateSpawnAmountMax)
            {
                plateSpawnAmount++;

                OnplateSpawn?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            if (plateSpawnAmount > 0)
            {
                plateSpawnAmount--;
                KitchenObject.SpawKitchenObject(plateKitchenObjectSO, player);

                OnplateRemove?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
