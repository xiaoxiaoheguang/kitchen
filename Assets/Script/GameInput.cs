using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 游戏输入系统类，处理玩家的所有输入操作
/// 实现单例模式，提供全局访问输入系统的能力
/// 支持自定义按键绑定，并将绑定数据持久化存储
/// </summary>
public class GameInput : MonoBehaviour
{
    #region 字段与属性
    /// <summary>
    /// 玩家按键绑定数据的 PlayerPrefs 键名
    /// 用于保存和加载自定义的按键配置
    /// </summary>
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    
    /// <summary>
    /// 输入系统单例实例，用于全局访问输入系统
    /// 在 Awake 中初始化，确保场景中只有一个输入系统实例
    /// </summary>
    public static GameInput Instance { get; private set; }

    /// <summary>
    /// 玩家主交互事件，当玩家按下交互键时触发
    /// 用于执行拾取、放置物品等操作
    /// </summary>
    public event EventHandler OnInteraction;
    
    /// <summary>
    /// 玩家备用交互事件，当玩家按下备用交互键时触发
    /// 用于执行切割、烹饪等操作
    /// </summary>
    public event EventHandler OnInteractionAlternate;
    
    /// <summary>
    /// 玩家暂停事件，当玩家按下暂停键时触发
    /// 用于打开或关闭暂停菜单
    /// </summary>
    public event EventHandler OnPauseAction;

    /// <summary>
    /// 按键绑定枚举，定义所有可自定义的输入操作
    /// 用于显示和修改按键绑定
    /// </summary>
    public enum Binding
    {
        Move_UP,
        Move_DOWN,
        Move_LEFT,
        Move_RIGHT,
        Interact,
        InteractAlt,
        Pause,
    }

    /// <summary>
    /// Unity 输入系统的 PlayerInputActions 实例
    /// 用于处理实际的输入检测和事件监听
    /// </summary>
    private PlayerInputActions playerInput;
    #endregion
    #region 生命周期方法
    /// <summary>
    /// 初始化输入系统单例和输入事件监听
    /// 功能：初始化单例、创建 PlayerInputActions、加载按键绑定、注册输入事件
    /// 参数：无
    /// 返回值：无
    /// </summary>
    private void Awake()
    {
        Instance = this;

        playerInput = new PlayerInputActions();
        playerInput.player.Enable();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInput.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        playerInput.player.Interact.performed += Interact_performed;
        playerInput.player.InteractAlternate.performed += InteractAlternate_performed;
        playerInput.player.Pause.performed += Pause_performed;
    }
    
    /// <summary>
    /// 清理输入系统资源，防止内存泄漏
    /// 功能：取消事件注册、释放 PlayerInputActions 资源
    /// 参数：无
    /// 返回值：无
    /// </summary>
    private void OnDestroy()
    {
        playerInput.player.Interact.performed -= Interact_performed;
        playerInput.player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInput.player.Pause.performed -= Pause_performed;

        playerInput.Dispose();
    }
    #endregion

    #region 输入事件处理
    /// <summary>
    /// 处理暂停输入事件
    /// 功能：当玩家按下暂停键时触发 OnPauseAction 事件
    /// 参数：obj - 输入回调上下文，包含输入操作信息
    /// 返回值：无
    /// </summary>
    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 处理备用交互输入事件
    /// 功能：当玩家按下备用交互键时触发 OnInteractionAlternate 事件
    /// 参数：obj - 输入回调上下文，包含输入操作信息
    /// 返回值：无
    /// </summary>
    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractionAlternate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 处理主交互输入事件
    /// 功能：当玩家按下主交互键时触发 OnInteraction 事件
    /// 参数：obj - 输入回调上下文，包含输入操作信息
    /// 返回值：无
    /// </summary>
    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteraction?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 获取玩家的移动向量（已归一化）
    /// 功能：读取输入系统的移动向量并归一化，确保斜向移动速度一致
    /// 参数：无
    /// 返回值：Vector2 - 归一化的移动向量，x 和 y 分量范围在 [-1, 1]
    /// </summary>
    public Vector2 GetMovement_Normalized_Vector2()
    {
        Vector2 move = playerInput.player.move.ReadValue<Vector2>();

        move = move.normalized;

        return move;
    }

    /// <summary>
    /// 获取指定按键绑定的显示文本
    /// 功能：根据 Binding 枚举返回对应的按键显示文本，用于 UI 显示
    /// 参数：binding - 要获取显示文本的按键绑定枚举
    /// 返回值：string - 按键绑定的显示文本（如 "W"、"Space" 等）
    /// </summary>
    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_UP:
                return playerInput.player.move.bindings[1].ToDisplayString();

            case Binding.Move_DOWN:
                return playerInput.player.move.bindings[2].ToDisplayString();

            case Binding.Move_LEFT:
                return playerInput.player.move.bindings[3].ToDisplayString();

            case Binding.Move_RIGHT:
                return playerInput.player.move.bindings[4].ToDisplayString();

            case Binding.Interact:
                return playerInput.player.Interact.bindings[0].ToDisplayString();

            case Binding.InteractAlt:
                return playerInput.player.InteractAlternate.bindings[0].ToDisplayString();

            case Binding.Pause:
                return playerInput.player.Pause.bindings[0].ToDisplayString();
        }
    }
    
    /// <summary>
    /// 重新绑定指定按键
    /// 功能：启用交互式按键绑定流程，等待玩家输入新按键后保存配置
    /// 实现思路：
    /// 1. 禁用输入操作，避免冲突
    /// 2. 根据 Binding 枚举确定要重新绑定的 InputAction 和索引
    /// 3. 启动交互式重新绑定流程
    /// 4. 绑定完成后保存配置到 PlayerPrefs
    /// 5. 启用输入操作并调用回调通知完成
    /// 参数：binding - 要重新绑定的按键枚举
    /// 参数：action - 绑定完成后的回调方法
    /// 返回值：无
    /// </summary>
    public void RebindBinding(Binding binding, Action action)
    {
        playerInput.player.Disable();

        InputAction inputAction = null;
        int index = 0;

        switch (binding)
        {
            case Binding.Move_UP:
                inputAction = playerInput.player.move;
                index = 1;
                break;
            case Binding.Move_DOWN:
                inputAction = playerInput.player.move;
                index = 2;
                break;
            case Binding.Move_LEFT:
                inputAction = playerInput.player.move;
                index = 3;
                break;
            case Binding.Move_RIGHT:
                inputAction = playerInput.player.move;
                index = 4;
                break;
            case Binding.Interact:
                inputAction = playerInput.player.Interact;
                index = 0;
                break;
            case Binding.InteractAlt:
                inputAction = playerInput.player.InteractAlternate;
                index = 0;
                break;
            case Binding.Pause:
                inputAction = playerInput.player.Pause;
                index = 0;
                break;
            default:
                break;
        }

        inputAction.PerformInteractiveRebinding(index)
            .OnComplete(callback =>
            {
                Debug.Log(callback.action.bindings[index].path);
                Debug.Log(callback.action.bindings[index].overridePath);
                callback.Dispose();
                playerInput.player.Enable();
                action();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInput.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            }).Start();
    }
    #endregion
}
