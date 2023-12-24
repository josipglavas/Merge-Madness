using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;

    [SerializeField] private AudioSource gameplaySource;
    [SerializeField] private AudioSource dropSource;
    [SerializeField] private AudioSource connectSource;

    [SerializeField] private AudioClip connectOrnamentClip;
    [SerializeField] private AudioClip dropOrnamentClip;


    private void Awake() {
        Instance = this;
    }

    public void PlayConnectOrnamentSound() {
        if (connectSource.isPlaying)
            connectSource.Pause();
        connectSource.pitch = (Random.Range(0.7f, 1.1f));
        connectSource.clip = connectOrnamentClip;
        connectSource.Play();
    }

    public void PlayDropOrnamentSound() {
        if (dropSource.isPlaying)
            dropSource.Pause();
        dropSource.pitch = (Random.Range(0.9f, 1.25f));
        dropSource.clip = dropOrnamentClip;
        dropSource.Play();
    }

    public void StopGameplayMusic() {
        gameplaySource.Stop();
    }

    public void SetGameplayMusicVolume(float volume) {
        gameplaySource.volume = volume * 0.35f; // we are multiplying by 0.35f because we dont want the sound to be too loud.
    }

    public void SetSFXVolume(float volume) {
        dropSource.volume = volume;
        connectSource.volume = volume;
    }

    public float GetSFXVolume() {
        return dropSource.volume;
    }

    public float GetMusicVolume() {
        return gameplaySource.volume;
    }
}
