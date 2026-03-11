using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChange += StoveCounter_OnProgressChange;
    }

    private void StoveCounter_OnProgressChange(object sender, IHasProgress.OnProgressChangeEventArgs e)
    {
        float burnShowProgressAmount = 0.5f;
        playWarningSound = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;

    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool playSound = e.State == StoveCounter.State.Cooking || e.State == StoveCounter.State.Cooked;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0f)
            {
                float warningSoundTimerMax = 0.2f;
                warningSoundTimer = warningSoundTimerMax;
                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
    }
}
