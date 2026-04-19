using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public event EventHandler OnScoreChanged;

    private List<RecipeSO> waitingRecipeSOList;
    private RecipeListSO currentLevelRecipeListSO;

    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;

    private int waitRecipeAmountMax = 4;
    private int successAmount = 0;
    private NetworkVariable<int> totalScore = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    public override void OnNetworkSpawn()
    {
        // 重置得分和等待食谱列表
        if (IsServer)
        {
            totalScore.Value = 0;
            successAmount = 0;
            waitingRecipeSOList.Clear();
        }

        // 从当前关卡获取食谱列表
        List<LevelSO> levelSoList = GameModeManager.Instance?.LevelSoList;
        int currentLevelIndex = KitchenMultiplayerGame.Instance?.currentLevelIndex.Value ?? 0;
        if (levelSoList != null && levelSoList.Count > currentLevelIndex)
        {
            currentLevelRecipeListSO = levelSoList[currentLevelIndex].levelRecipeListSO;
        }
    }

    private void Update()
    {
        if (!IsServer) return;


        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (GameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitRecipeAmountMax && currentLevelRecipeListSO != null)
            {
                int waittingRecipeSOIndex = UnityEngine.Random.Range(0, currentLevelRecipeListSO.recipeSOList.Count);

                SpawnNewWaittingRecipeClientRpc(waittingRecipeSOIndex);
                //Debug.Log(waitRecipeSO);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaittingRecipeClientRpc(int waittingRecipeSOIndex)
    {
        if (currentLevelRecipeListSO != null)
        {
            RecipeSO waitRecipeSO = currentLevelRecipeListSO.recipeSOList[waittingRecipeSOIndex];
            waitingRecipeSOList.Add(waitRecipeSO);

            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            //�������������ҵݽ��Ĳ����Ƿ��� �����б� ��ĳ����ͬ
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    // Cycling through all ingredients in the Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        // Cycling through all ingredients in the Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            // Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        // This Recipe ingredient was not found on the Plate
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    // Player delivered the correct recipe!
                    //Debug.Log("player deliver true recipe");

                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        DeliverIncorrectRecipeServerRpc();
        // No matches found!
        // Player did not deliver a correct recipe

    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waittingRecipeSOIndex)
    {
        // 在服务器端增加得分
        RecipeSO deliveredRecipe = waitingRecipeSOList[waittingRecipeSOIndex];
        totalScore.Value += deliveredRecipe.score;

        DeliverCorrectRecipeClientRpc(waittingRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waittingRecipeSOIndex)
    {
        successAmount++;

        OnScoreChanged?.Invoke(this, EventArgs.Empty);
        waitingRecipeSOList.RemoveAt(waittingRecipeSOIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }


    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
    public int GetSuccessRecipeSOCount()
    {
        return successAmount;
    }

    public int GetTotalScore()
    {
        return totalScore.Value;
    }
}