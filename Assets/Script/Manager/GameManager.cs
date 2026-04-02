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
    public event EventHandler OnPauseGame;
    public event EventHandler OnResumeGame;
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

    private NetworkVariable<State> state=new NetworkVariable<State>(State.WaitingToStart); 
    private bool isLocalPlayerReady = false;

    private Dictionary<ulong, bool> playerReadyMap;

    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);

    private NetworkVariable<float> gameplayTimer =new NetworkVariable<float>(0f);

    private float gameplayTimerMax = 10f;//300f;

    private bool isPause = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;

        playerReadyMap = new Dictionary<ulong, bool>();
    }


    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteraction += GameInput_OnInteraction;
    }

    private void Update()
    {
        if(!IsServer)
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

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged; 
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

            SetPlayerReadyServerRpc();

            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams=default) 
    {
        playerReadyMap[serverRpcParams.Receive.SenderClientId] = true;  

        bool isAllClientsReady =true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) 
        {
            if (!playerReadyMap.ContainsKey(clientId)|| !playerReadyMap[clientId])
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

    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
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
