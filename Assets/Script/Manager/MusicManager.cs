using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource musicSource;
    private float volume = .3f;
    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    private void Awake()
    {
        Instance = this;
        musicSource = GetComponent<AudioSource>();

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, .3f);
        musicSource.volume = volume;
    }
    private void Start()
    {
    }
    public void ChangeVolume()
    {
        volume += 0.1f;
        if (volume > 1.1f)
        {
            volume = 0f;
        }
        musicSource.volume = volume;
        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }
    public float GetVolume()
    {
        return volume;
    }
}
