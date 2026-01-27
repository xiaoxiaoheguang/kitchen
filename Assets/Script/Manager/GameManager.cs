using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChange;
    public event EventHandler OnPauseGame;
    public event EventHandler OnResumeGame;

    private enum State
    {
        WattingToStart,
        CountdownToStrat,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float waitingToStartTimer = 1f;
    private float countdownTimer = 3f;
    private float gameplayTimer;
    private float gameplayTimerMax = 60f;
    private bool isPause = false;

    private void Awake()
    {
        Instance = this;
        state = State.WattingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += Instance_OnPauseAction;
    }

    private void Instance_OnPauseAction(object sender, EventArgs e)
    {
        PauseGame();
    }

    private void Update()
    {
        switch (state)
        {
            case State.WattingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f)
                {
                    state = State.CountdownToStrat;
                    OnStateChange?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.CountdownToStrat:
                countdownTimer -= Time.deltaTime;
                if (countdownTimer < 0f)
                {
                    state = State.GamePlaying;
                    OnStateChange?.Invoke(this, EventArgs.Empty);

                    gameplayTimer = gameplayTimerMax;
                }
                break;
            case State.GamePlaying:
                gameplayTimer -= Time.deltaTime;
                if (gameplayTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChange?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:

                break;

        }
       // Debug.Log(state);
    }

    public void PauseGame()
    {
        isPause = !isPause;
        if (isPause)
        {
            Time.timeScale = 0f;
            OnPauseGame?.Invoke(this,EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnResumeGame?.Invoke(this,EventArgs.Empty);
        }
    }
    public bool IsGameCountdownActive()
    {
        return state == State.CountdownToStrat;
    }
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }
    public float GetCountdownTimer()
    {
        return countdownTimer;
    }
    public float GetGameplayTimerNormalized()
    {
        return 1-(gameplayTimer/gameplayTimerMax);
    }

}
