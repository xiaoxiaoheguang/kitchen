using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏管理器，负责控制游戏流程状态
/// </summary>
/// <remarks>
/// 管理游戏的等待开始、倒计时、游戏中、游戏结束等状态
/// 实现暂停/继续功能，并提供状态查询接口
/// </remarks>
public class GameManager : MonoBehaviour
{
    #region Singleton & Events

    /// <summary>
    /// 单例实例
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// 游戏状态变更事件
    /// </summary>
    public event EventHandler OnStateChanged;

    /// <summary>
    /// 游戏暂停事件
    /// </summary>
    public event EventHandler OnPauseGame;

    /// <summary>
    /// 游戏继续事件
    /// </summary>
    public event EventHandler OnResumeGame;

    #endregion

    #region State Enum

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    private enum State
    {
        /// <summary>等待开始状态</summary>
        WaitingToStart,
        /// <summary>倒计时开始状态</summary>
        CountdownToStart,
        /// <summary>游戏进行中状态</summary>
        GamePlaying,
        /// <summary>游戏结束状态</summary>
        GameOver,
    }

    #endregion

    #region Private Fields

    /// <summary>
    /// 当前游戏状态
    /// </summary>
    private State state;

    /// <summary>
    /// 倒计时计时器（秒）
    /// </summary>
    private float countdownTimer = 3f;

    /// <summary>
    /// 游戏进行计时器（秒）
    /// </summary>
    private float gameplayTimer;

    /// <summary>
    /// 游戏最大时长（秒）
    /// </summary>
    private float gameplayTimerMax = 300f;//todo :调试更改 原60f

    /// <summary>
    /// 游戏是否暂停
    /// </summary>
    private bool isPause = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初始化单例和初始状态
    /// </summary>
    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    /// <summary>
    /// 订阅输入事件
    /// </summary>
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteraction += GameInput_OnInteraction;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 处理交互输入事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    /// <remarks>
    /// 在等待开始状态下，按交互键开始倒计时
    /// </remarks>
    private void GameInput_OnInteraction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 处理暂停输入事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        PauseGame();
    }

    #endregion

    #region State Update

    /// <summary>
    /// 每帧更新游戏状态
    /// </summary>
    /// <remarks>
    /// 根据当前状态执行不同的逻辑：
    /// - 倒计时状态：减少倒计时，结束后进入游戏
    /// - 游戏中状态：减少游戏时间，结束后进入游戏结束
    /// </remarks>
    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                // 等待玩家交互开始游戏
                break;

            case State.CountdownToStart:
                // 倒计时逻辑
                countdownTimer -= Time.deltaTime;
                if (countdownTimer < 0f)
                {
                    state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                    gameplayTimer = gameplayTimerMax;
                }
                break;

            case State.GamePlaying:
                // 游戏进行逻辑
                gameplayTimer -= Time.deltaTime;
                if (gameplayTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GameOver:
                // 游戏结束，等待重新开始
                break;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 切换游戏暂停/继续状态
    /// </summary>
    /// <remarks>
    /// 通过修改 Time.timeScale 实现暂停效果
    /// 暂停时触发 OnPauseGame 事件，继续时触发 OnResumeGame 事件
    /// </remarks>
    public void PauseGame()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0f;
            OnPauseGame?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnResumeGame?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 检查是否处于倒计时开始状态
    /// </summary>
    /// <returns>如果处于倒计时状态返回 true，否则返回 false</returns>
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    /// <summary>
    /// 检查是否处于游戏进行中状态
    /// </summary>
    /// <returns>如果处于游戏进行中返回 true，否则返回 false</returns>
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    /// <summary>
    /// 检查是否处于游戏结束状态
    /// </summary>
    /// <returns>如果处于游戏结束返回 true，否则返回 false</returns>
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    /// <summary>
    /// 获取当前倒计时剩余时间
    /// </summary>
    /// <returns>倒计时剩余时间（秒）</returns>
    public float GetCountdownTimer()
    {
        return countdownTimer;
    }

    /// <summary>
    /// 获取游戏进行时间的归一化值
    /// </summary>
    /// <returns>归一化的游戏时间（0.0 到 1.0，1.0 表示时间用完）</returns>
    /// <remarks>
    /// 返回值 = 1 - (当前时间 / 总时间)，便于进度条显示
    /// </remarks>
    public float GetGameplayTimerNormalized()
    {
        return 1 - (gameplayTimer / gameplayTimerMax);
    }

    #endregion

}
