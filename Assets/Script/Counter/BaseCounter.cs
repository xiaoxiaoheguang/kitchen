using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 柜台基类，所有柜台类型（清除柜台、容器柜台、切割柜台等）的基类
/// 实现 IKitchenObjectParent 接口，支持厨房物品的放置和获取
/// 提供虚拟方法供子类重写交互逻辑
/// </summary>
public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    #region 静态成员
    /// <summary>
    /// 全局静态事件，当任何柜台上放置物品时触发
    /// 用于音效或视觉反馈等全局效果
    /// </summary>
    public static event EventHandler OnAnyObjectSethere;

    /// <summary>
    /// 重置静态数据
    /// 功能：清空静态事件的所有订阅，防止场景切换时的内存泄漏
    /// 参数：无
    /// 返回值：无
    /// </summary>
    public static void ResetStaticData()
    {
        OnAnyObjectSethere = null;
    }
    #endregion

    #region 字段与属性
    /// <summary>
    /// 柜台顶部的 Transform，用于放置厨房物品
    /// 物品会被放置在这个位置
    /// </summary>
    [Tooltip("柜台顶部物品放置点")]
    [SerializeField] private Transform counterTopPoint;
    
    /// <summary>
    /// 当前柜台上的厨房物品对象
    /// 为 null 时表示柜台上没有物品
    /// </summary>
    private KitchenObject KitchenObject;
    #endregion

    #region 虚方法
    /// <summary>
    /// 主交互方法，子类重写实现具体交互逻辑
    /// 功能：处理玩家与柜台的主交互（拾取、放置物品等）
    /// 参数：player - 与柜台交互的玩家对象
    /// 返回值：无
    /// </summary>
    public virtual void Interact(Player player)
    {
        Debug.Log("Interact virtual");
    }
    
    /// <summary>
    /// 备用交互方法，子类重写实现具体备用交互逻辑
    /// 功能：处理玩家与柜台的备用交互（切割、烹饪等）
    /// 参数：player - 与柜台交互的玩家对象
    /// 返回值：无
    /// </summary>
    public virtual void InteractAlternate(Player player)
    {
        Debug.Log("InteractAlternate virtual");
    }
    #endregion

    #region IKitchenObjectParent 接口实现
    /// <summary>
    /// 获取厨房物品跟随的 Transform 位置
    /// 功能：返回物品应该放置的 Transform，用于物品定位
    /// 参数：无
    /// 返回值：Transform - 柜台顶部物品放置点的 Transform
    /// </summary>
    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    /// <summary>
    /// 设置柜台上的厨房物品
    /// 功能：更新柜台上的物品对象，如果物品不为空则触发全局放置事件
    /// 参数：kitchenObject - 要放置在柜台上的厨房物品对象，传入 null 表示清空物品
    /// 返回值：无
    /// </summary>
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.KitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnAnyObjectSethere?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// 获取柜台上的厨房物品
    /// 功能：返回当前柜台上的物品对象
    /// 参数：无
    /// 返回值：KitchenObject - 柜台上的物品，没有物品时返回 null
    /// </summary>
    public KitchenObject GetKitchenObject()
    {
        return KitchenObject;
    }
    
    /// <summary>
    /// 清除柜台上的厨房物品
    /// 功能：将柜台上的物品设置为 null，表示柜台上不再有物品
    /// 参数：无
    /// 返回值：无
    /// </summary>
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }
    
    /// <summary>
    /// 判断柜台上是否有厨房物品
    /// 功能：检查柜台上是否持有物品
    /// 参数：无
    /// 返回值：bool - true 表示柜台上有物品，false 表示没有
    /// </summary>
    public bool HasKitchenObject()
    {
        return (KitchenObject != null);
    }

public NetworkObject GetNetworkObject()
    {
        return null;
    }
    #endregion
}
