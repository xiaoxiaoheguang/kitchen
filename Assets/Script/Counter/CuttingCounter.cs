using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 切割柜台类，用于将食材切割成更小的部分
/// 继承自 BaseCounter，实现 IHasProgress 接口显示切割进度
/// 支持将可切割的食材放置在柜台上，通过备用交互进行切割
/// </summary>
public class CuttingCounter : BaseCounter, IHasProgress
{
    #region 静态成员
    /// <summary>
    /// 全局静态事件，当任何切割柜台进行切割时触发
    /// 用于音效或视觉反馈等全局效果
    /// </summary>
    public static event EventHandler OnAnyCut;

    /// <summary>
    /// 重置静态数据
    /// 功能：清空静态事件的所有订阅，防止场景切换时的内存泄漏
    /// 参数：无
    /// 返回值：无
    /// </summary>
    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }
    #endregion

    #region 字段与属性
    /// <summary>
    /// 进度改变事件，当切割进度变化时触发
    /// 用于更新进度条 UI 显示
    /// </summary>
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChange;

    /// <summary>
    /// 切割事件，当进行一次切割时触发
    /// 用于播放切割动画或音效
    /// </summary>
    public event EventHandler OnCut;

    /// <summary>
    /// 切割配方数组，定义所有可切割的食材及其切割后的产物
    /// </summary>
    [Tooltip("切割配方数组")]
    [SerializeField] private CuttingRecipeSO[] CuttingKitchenObjectSOArray;

    /// <summary>
    /// 当前切割进度，记录已完成的切割次数
    /// </summary>
    private int cuttingProgress;
    #endregion

    #region 重写方法
    /// <summary>
    /// 主交互方法，处理玩家与切割柜台的物品交换
    /// 功能：处理玩家与柜台之间的物品放置、拾取和盘子食材添加逻辑
    /// 实现思路：
    /// 1. 如果柜台没有物品且玩家持有物品，将物品放到柜台上
    /// 2. 如果物品可切割，初始化切割进度
    /// 3. 如果柜台有物品且玩家没有物品，将物品给玩家
    /// 4. 如果双方都有物品，尝试将食材添加到盘子上
    /// 参数：player - 与柜台交互的玩家对象
    /// 返回值：无
    /// </summary>
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // 柜台上没有物品，玩家有物品，放置物品到柜台
            if (player.HasKitchenObject())
            {
                // 检查物品是否可切割
                if (HasRecipeInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // 将玩家持有的物品放到柜台上
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            }
            else
            {
                // 柜台和玩家都没有物品，不做任何操作
            }
        }
        else
        {
            // 柜台上有物品，玩家没有物品，拾取柜台上的物品
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                // 双方都有物品，尝试将食材添加到盘子上
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 备用交互方法，执行切割操作
    /// 功能：增加切割进度，触发切割事件，完成后生成切割后的物品
    /// 实现思路：
    /// 1. 检查柜台上是否有可切割的物品
    /// 2. 增加切割进度
    /// 3. 触发切割事件和全局切割事件
    /// 4. 更新进度显示
    /// 5. 如果进度达标，销毁原物品并生成切割后的物品
    /// 参数：player - 与柜台交互的玩家对象
    /// 返回值：无
    /// </summary>
    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();
            TestCuttingProgressServerRpc();
        }
    }
    #endregion

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;

        OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalized = 0f
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress += 1;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChange?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });


    }
    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOFromInput(GetKitchenObject().GetKitchenObjectSO());

        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO outputSO = GetOutputForINput(GetKitchenObject().GetKitchenObjectSO());

            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpawKitchenObject(outputSO, this);
        }
    }

    #region 私有方法

    /// <summary>
    /// 检查是否有对应输入食材的切割配方
    /// 功能：判断给定的食材是否可以被切割
    /// 参数：inputKitchenObjectSO - 要检查的食材对象
    /// 返回值：bool - true 表示有对应的切割配方，false 表示没有
    /// </summary>
    private bool HasRecipeInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO outputCuttingRecipeSO = GetCuttingRecipeSOFromInput(inputKitchenObjectSO);
        return (outputCuttingRecipeSO != null);
    }

    /// <summary>
    /// 获取输入食材切割后的输出食材
    /// 功能：根据输入食材查找对应的切割配方并返回输出食材
    /// 参数：inputKitchenObjectSO - 输入的食材对象
    /// 返回值：KitchenObjectSO - 切割后的食材对象，没有对应配方时返回 null
    /// </summary>
    private KitchenObjectSO GetOutputForINput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO outputCuttingRecipeSO = GetCuttingRecipeSOFromInput(inputKitchenObjectSO);
        if (outputCuttingRecipeSO != null)
        {
            return outputCuttingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 根据输入食材获取对应的切割配方
    /// 功能：遍历切割配方数组，查找匹配的输入食材的配方
    /// 参数：inputKitchenObjectSO - 输入的食材对象
    /// 返回值：CuttingRecipeSO - 匹配的切割配方，没有找到时返回 null
    /// </summary>
    private CuttingRecipeSO GetCuttingRecipeSOFromInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in CuttingKitchenObjectSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
    #endregion
}
