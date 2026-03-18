using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CuttingCounter;
using static IHasProgress;

/// <summary>
/// 煎制柜台类，负责处理食材的烹饪和烧焦逻辑
/// </summary>
/// <remarks>
/// 继承自 BaseCounter，实现 IHasProgress 接口
/// 支持食材烹饪、煎好后继续加热会烧焦的功能
/// </remarks>
public class StoveCounter : BaseCounter, IHasProgress
{
    #region Serialized Fields

    /// <summary>
    /// 烹饪配方数组
    /// </summary>
    [SerializeField] private CookingRecipeSO[] CookingRecipeArray;

    #endregion

    #region Events

    /// <summary>
    /// 进度变化事件
    /// </summary>
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChange;

    /// <summary>
    /// 状态变化事件
    /// </summary>
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    /// <summary>
    /// 状态变化事件参数
    /// </summary>
    public class OnStateChangedEventArgs : EventArgs
    {
        /// <summary>当前状态</summary>
        public State State;
    }

    #endregion

    #region State Enum

    /// <summary>
    /// 烹饪状态枚举
    /// </summary>
    public enum State
    {
        /// <summary>空闲状态</summary>
        Idle,
        /// <summary>烹饪中状态</summary>
        Cooking,
        /// <summary>煎好状态</summary>
        Cooked,
        /// <summary>烧焦状态</summary>
        Burned,
    }

    #endregion

    #region Private Fields

    /// <summary>
    /// 当前烹饪状态
    /// </summary>
    private State state;

    /// <summary>
    /// 烹饪计时器
    /// </summary>
    [SerializeField] private float cookingTimer;

    /// <summary>
    /// 烧焦计时器
    /// </summary>
    [SerializeField] float burningTimer;

    /// <summary>
    /// 当前使用的烹饪配方
    /// </summary>
    private CookingRecipeSO cookingRecipeSO;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初始化状态为空闲
    /// </summary>
    private void Start()
    {
        state = State.Idle;
    }

    /// <summary>
    /// 每帧更新烹饪状态
    /// </summary>
    /// <remarks>
    /// 根据当前状态执行不同逻辑：
    /// - 烹饪中：增加计时，完成后切换到煎好状态
    /// - 煎好：增加计时，完成后切换到烧焦状态
    /// </remarks>
    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    // 空闲状态，不做处理
                    break;

                case State.Cooking:
                    // 烹饪逻辑
                    cookingTimer += Time.deltaTime;

                    // 更新进度
                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = cookingTimer / cookingRecipeSO.cookingTimeMax
                    });

                    // 检查是否烹饪完成
                    if (cookingTimer > cookingRecipeSO.cookingTimeMax)
                    {
                        cookingTimer = 0f;

                        // 销毁原始物品，生成煎好的物品
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawKitchenObject(cookingRecipeSO.output, this);

                        // 切换到煎好状态
                        state = State.Cooked;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = state
                        });
                    }

                    // 初始化烧焦计时和配方
                    burningTimer = 0f;
                    cookingRecipeSO = GetCookingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());

                    break;

                case State.Cooked:
                    // 烧焦逻辑
                    burningTimer += Time.deltaTime;

                    // 更新进度
                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = burningTimer / cookingRecipeSO.cookingTimeMax
                    });

                    // 检查是否烧焦完成
                    if (burningTimer > cookingRecipeSO.cookingTimeMax)
                    {
                        burningTimer = 0f;

                        // 销毁煎好的物品，生成烧焦的物品
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawKitchenObject(cookingRecipeSO.output, this);

                        // 切换到烧焦状态
                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            State = state
                        });

                        // 清空进度
                        OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }

                    break;

                case State.Burned:
                    // 烧焦状态，不做处理
                    break;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 处理玩家与煎制柜台的交互
    /// </summary>
    /// <param name="player">交互的玩家对象</param>
    /// <remarks>
    /// 交互逻辑：
    /// 1. 柜台无物品且玩家有可烹饪物品：开始烹饪
    /// 2. 柜台有物品且玩家无物品：玩家取走物品
    /// 3. 柜台有物品且玩家有盘子：将物品放到盘子上
    /// </remarks>
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // 柜台没有物品
            if (player.HasKitchenObject())
            {
                if (HasRecipeInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // 玩家拿着可以烹饪的物品
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    // 初始化烹饪状态
                    cookingRecipeSO = GetCookingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());
                    state = State.Cooking;
                    cookingTimer = 0f;

                    // 触发状态变化事件
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        State = state
                    });

                    // 初始化进度
                    OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
                    {
                        progressNormalized = cookingTimer / cookingRecipeSO.cookingTimeMax
                    });
                }
            }
        }
        else
        {
            // 柜台有物品
            if (!player.HasKitchenObject())
            {
                // 玩家没拿物品，取走柜台物品
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
                // 玩家拿着物品
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // 玩家拿着盘子
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // 将柜台物品添加到盘子上
                        GetKitchenObject().DestroySelf();

                        // 重置状态
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

    /// <summary>
    /// 备用交互方法（当前未实现）
    /// </summary>
    /// <param name="player">交互的玩家对象</param>
    public override void InteractAlternate(Player player)
    {
        // 备用交互功能暂未实现
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 检查是否有对应输入的烹饪配方
    /// </summary>
    /// <param name="inputKitchenObjectSO">输入的厨房物品</param>
    /// <returns>如果有对应配方返回 true，否则返回 false</returns>
    private bool HasRecipeInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CookingRecipeSO outputCookingRecipeSO = GetCookingRecipeSOFromInput(inputKitchenObjectSO);
        return (outputCookingRecipeSO != null);
    }

    /// <summary>
    /// 获取输入物品对应的烹饪输出物品
    /// </summary>
    /// <param name="inputKitchenObjectSO">输入的厨房物品</param>
    /// <returns>输出的厨房物品，如果没有配方返回 null</returns>
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

    /// <summary>
    /// 根据输入物品获取烹饪配方
    /// </summary>
    /// <param name="inputKitchenObjectSO">输入的厨房物品</param>
    /// <returns>对应的烹饪配方，如果没有找到返回 null</returns>
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

    /// <summary>
    /// 检查当前是否处于煎好状态
    /// </summary>
    /// <returns>如果处于煎好状态返回 true，否则返回 false</returns>
    public bool IsFried()
    {
        return state == State.Cooked;
    }

    #endregion
}
