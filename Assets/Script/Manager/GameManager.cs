using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// 游戏管理器，负责控制游戏流程状态
/// </summary>
/// <remarks>
/// 管理游戏的等待开始、倒计时、游戏中、游戏结束等状态
/// 实现暂停/继续功能，并提供状态查询接口
/// </remarks>
public class GameManager : NetworkBehaviour
{
    #region Singleton & Events

    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePause;
    public event EventHandler OnLocalGameResume;
    public event EventHandler OnMultiplayGamePause;
    public event EventHandler OnMultiplayGameResume;

    public event EventHandler OnLocalPlayerReadyChanged;

    #endregion

    #region State Enum

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    #endregion

    #region Private Fields

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    private bool isLocalPlayerReady = false;
    private bool isLocalGamePause = false;

    private NetworkVariable<bool> isPause = new NetworkVariable<bool>(false);

    private Dictionary<ulong, bool> playerReadyMap;
    private Dictionary<ulong, bool> playerPausedMap;

    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gameplayTimer = new NetworkVariable<float>(0f);

    private float gameplayTimerMax = 90f;

    private bool autoTestGamePauseState;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;

        playerReadyMap = new Dictionary<ulong, bool>();
        playerPausedMap = new Dictionary<ulong, bool>();
    }


    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteraction += GameInput_OnInteraction;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownTimer.Value -= Time.deltaTime;
                if (countdownTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gameplayTimer.Value = gameplayTimerMax;
                }
                break;

            case State.GamePlaying:
                gameplayTimer.Value -= Time.deltaTime;
                if (gameplayTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;

            case State.GameOver:
                break;
        }
    }

    private void LateUpdate()
    {
        if (autoTestGamePauseState)
        {
            autoTestGamePauseState = false;
            TestGamePauseState();
        }
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isPause.OnValueChanged += IsPause_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManger_ClientConnectedCallback;
        }
    }

    private void NetworkManger_ClientConnectedCallback(ulong obj)
    {
        autoTestGamePauseState = true;
    }

    private void IsPause_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isPause.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayGamePause?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayGameResume?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State oldValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Event Handlers

    private void GameInput_OnInteraction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyMap[serverRpcParams.Receive.SenderClientId] = true;

        bool isAllClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyMap.ContainsKey(clientId) || !playerReadyMap[clientId])
            {
                isAllClientsReady = false;
                break;
            }
        }

        if (isAllClientsReady)
        {
            state.Value = State.CountdownToStart;
        }

    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        PauseGame();
    }

    #endregion

    #region Public Methods

    public void PauseGame()
    {
        isLocalGamePause = !isLocalGamePause;
        if (isLocalGamePause)
        {
            PauseGameServerRpc();

            OnLocalGamePause?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ResumeGameServerRpc();

            OnLocalGameResume?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedMap[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePauseState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResumeGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPausedMap[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();
    }

    private void TestGamePauseState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedMap.ContainsKey(clientId) && playerPausedMap[clientId])
            {
                // 只要有一个玩家处于暂停状态，游戏就应该处于暂停状态
                isPause.Value = true;
                return;
            }
        }
        // 没有玩家处于暂停状态，游戏应该继续
        isPause.Value = false;
    }


    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }
    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public float GetCountdownTimer()
    {
        return countdownTimer.Value;
    }

    public float GetGameplayTimerNormalized()
    {
        return 1 - (gameplayTimer.Value / gameplayTimerMax);
    }

    #endregion

}
