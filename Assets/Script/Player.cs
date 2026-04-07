using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家核心控制类，管理玩家的移动、交互和厨房物品持有逻辑
/// 实现单例模式，提供全局访问玩家对象的能力
/// 实现 IKitchenObjectParent 接口，支持厨房物品的拾取和放置
/// </summary>
public class Player : NetworkBehaviour, IKitchenObjectParent
{
    /// <summary>
    /// 玩家单例实例，用于全局访问玩家对象
    /// 在 Awake 中初始化，确保场景中只有一个玩家实例
    /// </summary>
    public static Player LocalInstance { get; private set; }

    #region 字段与属性
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    /// <summary>
    /// 玩家移动速度，单位：米/秒
    /// 用于控制玩家在场景中的移动速率
    /// </summary>
    [Tooltip("玩家移动速度（米/秒）")]
    [SerializeField] private float moveSpeed = 5f;

    /// <summary>
    /// 厨房物品持有点的 Transform，用于物品跟随玩家移动
    /// 物品会被放置在这个位置并随玩家移动
    /// </summary>
    [Tooltip("厨房物品持有点位置")]
    [SerializeField] private Transform kitchenObjectHoldPoint;


    /// <summary>
    /// 当前玩家持有的厨房物品对象
    /// 为 null 时表示玩家未持有任何物品
    /// </summary>
    [SerializeField] private KitchenObject KitchenObject;

    [SerializeField] private List<Vector3> spawnPositionList;

    /// <summary>
    /// 当前玩家选中的柜台对象
    /// 用于执行交互操作（拾取、放置、切割等）
    /// </summary>
    private BaseCounter selectCounter;

    /// <summary>
    /// 玩家拾取物品事件，当玩家成功拾取物品时触发
    /// 参数：sender 为事件源，EventArgs 为空参数
    /// </summary>
    public event EventHandler OnPickSomething;

    /// <summary>
    /// 选中柜台改变事件，当玩家选中或取消选中柜台时触发
    /// 参数：sender 为事件源，OnSelectedCounterChangedEventArgs 包含选中的柜台信息
    /// </summary>
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    /// <summary>
    /// 选中柜台改变事件的参数类
    /// 继承自 EventArgs，用于传递选中的柜台对象
    /// </summary>
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }


    static public event EventHandler OnAnyPlayerSpawned;
    static public event EventHandler OnAnyPickedSomething;

    /// <summary>
    /// 玩家是否正在移动的标志
    /// 用于动画系统控制播放状态
    /// </summary>
    private bool isWalking = false;
    #endregion

    #region 生命周期方法
    /// <summary>
    /// 初始化玩家单例实例
    /// 功能：确保场景中只有一个玩家实例，如果已存在则输出错误日志
    /// 参数：无
    /// 返回值：无
    /// </summary>
    private void Awake()
    {
        //if (instance != null)
        //{
        //    Debug.LogError("more instence");
        //}
        //instance = this;
    }

    override public void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObj(GetKitchenObj());
        }
    }

    /// <summary>
    /// 游戏开始时注册输入事件监听
    /// 功能：订阅游戏输入系统的交互和备用交互事件
    /// 参数：无
    /// 返回值：无
    /// </summary>
    void Start()
    {
        GameInput.Instance.OnInteraction += GameInput_OnInteraction;
        GameInput.Instance.OnInteractionAlternate += GameInput_OnInteractionAlternate;
    }

    /// <summary>
    /// 每帧更新玩家状态
    /// 功能：处理玩家移动和与柜台的交互检测
    /// 参数：无
    /// 返回值：无
    /// </summary>
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleMovement();
        HandleInteractions();
    }
    #endregion

    #region 输入事件处理
    /// <summary>
    /// 处理备用交互输入事件
    /// 功能：当玩家按下备用交互键时，对当前选中的柜台执行备用交互操作（如切割、烹饪等）
    /// 参数：sender - 事件源对象，e - 事件参数
    /// 返回值：无
    /// </summary>
    private void GameInput_OnInteractionAlternate(object sender, EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectCounter != null)
        {
            selectCounter.InteractAlternate(this);
        }
    }

    /// <summary>
    /// 处理主交互输入事件
    /// 功能：当玩家按下主交互键时，对当前选中的柜台执行主交互操作（如拾取、放置物品等）
    /// 参数：sender - 事件源对象，e - 事件参数
    /// 返回值：无
    /// </summary>
    private void GameInput_OnInteraction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectCounter != null)
        {
            selectCounter.Interact(this);
        }
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 获取玩家是否正在移动
    /// 功能：返回玩家当前的移动状态，用于动画系统控制
    /// 参数：无
    /// 返回值：bool - true 表示正在移动，false 表示静止
    /// </summary>
    public bool IsWalking()
    {
        return isWalking;
    }

    static public void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 上一次交互方向，用于玩家停止移动时仍能交互前方物体
    /// 当玩家停止移动时，保持最后一次的移动方向用于交互检测
    /// </summary>
    private Vector3 lastInteractionDir;

    /// <summary>
    /// 设置当前选中的柜台并触发事件
    /// 功能：更新选中的柜台对象，并触发 OnSelectedCounterChanged 事件通知其他系统
    /// 参数：baseCounter - 要设置的柜台对象，传入 null 表示取消选中
    /// 返回值：无
    /// </summary>
    private void SetSelectCounter(BaseCounter baseCounter)
    {
        selectCounter = baseCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectCounter,
        });
    }

    /// <summary>
    /// 处理玩家与柜台的交互检测
    /// 功能：通过射线检测玩家前方是否有可交互的柜台，更新选中的柜台状态
    /// 实现思路：使用 Physics.Raycast 从玩家位置向移动方向发射射线，检测是否碰到柜台
    /// 参数：无
    /// 返回值：无
    /// </summary>
    public void HandleInteractions()
    {
        Vector2 InputVector = GameInput.Instance.GetMovement_Normalized_Vector2();
        Vector3 movedir = new Vector3(InputVector.x, 0f, InputVector.y);

        if (movedir != Vector3.zero)
        {
            lastInteractionDir = movedir;
        }

        float interactDistance = 1f;
        if (Physics.Raycast(
            transform.position,
            lastInteractionDir,
            out RaycastHit raycastHit,
            interactDistance,
            countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter basecounter))
            {
                if (basecounter != selectCounter)
                {
                    SetSelectCounter(basecounter);
                }
            }
            else
            {
                SetSelectCounter(null);
            }
        }
        else
        {
            SetSelectCounter(null);
        }
    }

    /// <summary>
    /// 处理玩家移动逻辑
    /// 功能：根据输入向量移动玩家，使用胶囊体碰撞检测实现平滑移动和滑动效果
    /// 实现思路：
    /// 1. 获取输入向量并转换为 3D 移动方向
    /// 2. 使用 CapsuleCast 检测前方是否有障碍物
    /// 3. 如果前方有障碍，尝试沿 X 轴或 Z 轴滑动
    /// 4. 使用 Slerp 平滑旋转玩家朝向移动方向
    /// 参数：无
    /// 返回值：无
    /// </summary>
    private void HandleMovement()
    {
        Vector2 InputVector = GameInput.Instance.GetMovement_Normalized_Vector2();
        Vector3 movedir = new Vector3(InputVector.x, 0f, InputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .6f;
        float playHeight = 2f;

        bool canmove = !Physics.BoxCast(transform.position,
            playerRadius * Vector3.one,
            movedir,
            Quaternion.identity,
            moveDistance,
            collisionsLayerMask);

        if (!canmove)
        {
            Vector3 movedirX = new Vector3(movedir.x, 0, 0);

            bool canmoveX = Mathf.Abs(movedir.x) > 0.5f
                && !Physics.BoxCast(
                transform.position,
                Vector3.one * playerRadius,
                movedirX,
                Quaternion.identity,
                moveDistance,
                collisionsLayerMask);

            if (canmoveX)
            {
                transform.position += movedirX * Time.deltaTime * moveSpeed;
            }
            else
            {
                Vector3 movedirZ = new Vector3(0, 0, movedir.z);

                bool canmoveZ = Mathf.Abs(movedir.z) > 0.5f
                    && !Physics.BoxCast(
                    transform.position,
                    Vector3.one * playerRadius,
                    movedirZ,
                    Quaternion.identity,
                    moveDistance,
                    collisionsLayerMask);

                if (canmoveZ)
                {
                    transform.position += movedirZ * Time.deltaTime * moveSpeed;
                }
            }
        }
        else
        {
            transform.position += movedir * Time.deltaTime * moveSpeed;
        }

        isWalking = movedir != Vector3.zero;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movedir, Time.deltaTime * rotationSpeed);
    }
    #endregion

    #region IKitchenObjectParent 接口实现
    /// <summary>
    /// 获取厨房物品跟随的 Transform 位置
    /// 功能：返回物品应该跟随的 Transform，用于物品定位
    /// 参数：无
    /// 返回值：Transform - 厨房物品持有点的 Transform
    /// </summary>
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    /// <summary>
    /// 设置当前持有的厨房物品
    /// 功能：更新玩家持有的物品对象，如果物品不为空则触发拾取事件
    /// 参数：kitchenObject - 要设置的厨房物品对象，传入 null 表示清空物品
    /// 返回值：无
    /// </summary>
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.KitchenObject = kitchenObject;

        if (this.KitchenObject != null)
        {
            OnPickSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 获取当前持有的厨房物品
    /// 功能：返回玩家当前持有的物品对象
    /// 参数：无
    /// 返回值：KitchenObject - 当前持有的物品，未持有时返回 null
    /// </summary>
    public KitchenObject GetKitchenObj()
    {
        return KitchenObject;
    }

    /// <summary>
    /// 清除当前持有的厨房物品
    /// 功能：将玩家持有的物品设置为 null，表示不再持有任何物品
    /// 参数：无
    /// 返回值：无
    /// </summary>
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }

    /// <summary>
    /// 判断当前是否持有厨房物品
    /// 功能：检查玩家是否持有物品
    /// 参数：无
    /// 返回值：bool - true 表示持有物品，false 表示未持有
    /// </summary>
    public bool HasKitchenObject()
    {
        return (KitchenObject != null);
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    #endregion
}
