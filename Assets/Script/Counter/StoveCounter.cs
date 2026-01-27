using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CuttingCounter;
using static IHasProgress;

public class StoveCounter : BaseCounter,IHasProgress
{
    [SerializeField] private CookingRecipeSO[] CookingRecipeArray;


    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChange;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State State;
    }

    public enum State
    {
        Idle,
        Cooking,
        Cooked,
        Burned,
    }

    private State state;
    [SerializeField] private float cookingTimer;
    [SerializeField] float burningTimer;
    private CookingRecipeSO cookingRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }
    private void Update()
    {
        if (HasKitchenObject())
        {

            switch (state)
            {
                case State.Idle:
                    break;
                case State.Cooking:
                    cookingTimer += Time.deltaTime;
                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = cookingTimer / cookingRecipeSO.cookingTimeMax
                    });
                    if (cookingTimer > cookingRecipeSO.cookingTimeMax)
                    {
                        cookingTimer = 0f;

                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawKitchenObject(cookingRecipeSO.output, this);

                        state = State.Cooked;

                         OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                         State = state
                         });
                        //Debug.Log("cook");
                    }

                    burningTimer = 0f;
                    cookingRecipeSO = GetCookingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());

                    break;
                case State.Cooked:

                    burningTimer += Time.deltaTime;
                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = burningTimer / cookingRecipeSO.cookingTimeMax
                    });
                    if (burningTimer > cookingRecipeSO.cookingTimeMax)
                    {
                        burningTimer = 0f;

                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawKitchenObject(cookingRecipeSO.output, this);

                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = state
                        });

                        OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                        {
                            progressNormalized = 0f
                        });
                        //Debug.Log("burn");
                    }

                    break;
                case State.Burned:
                    break;
            }

            //Debug.Log(state);
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //counter no object
            if (player.HasKitchenObject())
            {
                if (HasRecipeInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    cookingRecipeSO = GetCookingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());

                    state = State.Cooking;
                    cookingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        State = state
                    });

                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = cookingTimer / cookingRecipeSO.cookingTimeMax
                    });
                }
            }
            else
            {
            }
        }
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    State = state
                });
                OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                {
                    progressNormalized = 0f
                });

            }
            else
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = state
                        });
                        OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                        {
                            progressNormalized = 0f
                        });

                    }
                }
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        //if (HasKitchenObject() && HasRecipeInput(GetKitchenObject().GetKitchenObjectSO()))
        //{
        //    cuttingProgress += 1;

        //    OnCut?.Invoke(this, EventArgs.Empty);

        //    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());
        //    OnProgressChange?.Invoke(this, new OnProgressChangeEventArgs
        //    {
        //        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        //    });
        //    if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        //    {
        //        KitchenObjectSO outputSO = GetOutputForINput(GetKitchenObject().GetKitchenObjectSO());
        //        GetKitchenObject().DestroySelf();

        //        KitchenObject.SpawKitchenObject(outputSO, this);
        //    }
        //}
    }

    private bool HasRecipeInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CookingRecipeSO outputCookingRecipeSO = GetCookingRecipeSOFromInput(inputKitchenObjectSO);
        return (outputCookingRecipeSO != null);

    }
    private KitchenObjectSO GetOutputForINput(KitchenObjectSO inputKitchenObjectSO)
    {
        CookingRecipeSO outputCookingRecipeSO = GetCookingRecipeSOFromInput(inputKitchenObjectSO);
        if (outputCookingRecipeSO != null)
        {
            return outputCookingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }
    private CookingRecipeSO GetCookingRecipeSOFromInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CookingRecipeSO cookingRecipeSO in CookingRecipeArray)
        {
            if (cookingRecipeSO.input == inputKitchenObjectSO)
            {
                return cookingRecipeSO;
            }
        }
        return null;
    }
}
