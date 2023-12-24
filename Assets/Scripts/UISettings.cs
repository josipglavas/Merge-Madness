using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettings : MonoBehaviour {

    [SerializeField] private Button showSettingsButton;
    [SerializeField] private Button hideSettingsButton;
    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private Button gameplayMusicButton;
    [SerializeField] private Slider gameplayMusicSlider;
    [SerializeField] private Image gameplayMusicSpeakerImage;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Image sfxSpeakerImage;
    [SerializeField] private Sprite speakerOn;
    [SerializeField] private Sprite speakerOff;
    [SerializeField] private Sprite noteOn;
    [SerializeField] private Sprite noteOff;
    [SerializeField] private TextMeshProUGUI highestScoreText;

    private float lastGameplayMusicVolume = 0.5f;
    private float lastSFXVolume = 1.0f;
    private bool gameplayMusicPlaying = true;
    private bool sfxPlaying = true;
    private const string url = "https://mergemadness.josipglavas.com/privacy";


    private void Awake() {
        showSettingsButton.onClick.AddListener(() => {
            ShowSettings();
        });

        hideSettingsButton.onClick.AddListener(() => {
            HideSettings();
        });

        gameplayMusicButton.onClick.AddListener(() => {
            if (gameplayMusicPlaying && AudioManager.Instance.GetMusicVolume() > 0.0f) {
                SetGameplayMusicVolume(0.0f);
                gameplayMusicPlaying = false;
            } else {
                SetGameplayMusicVolume(lastGameplayMusicVolume);
                gameplayMusicPlaying = true;
            }
        });

        sfxButton.onClick.AddListener(() => {
            if (sfxPlaying && AudioManager.Instance.GetSFXVolume() > 0.0f) {
                SetSFXVolume(0.0f);
                sfxPlaying = false;
            } else {
                SetSFXVolume(lastSFXVolume);
                sfxPlaying = true;
            }
        });

        gameplayMusicSlider.onValueChanged.AddListener((float volume) => {
            SetGameplayMusicVolume(volume);
        });

        sfxSlider.onValueChanged.AddListener((float volume) => {
            SetSFXVolume(volume);
        });

        privacyPolicyButton.onClick.AddListener(() => {
            ShowPrivacyPolicy();
        });

        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;
    }

    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e) {
        showSettingsButton.interactable = false;
    }

    private void Start() {
        HideSettings();
        SetSFXVolume(FileManager.Instance.SfxVolume, false);
        SetGameplayMusicVolume(FileManager.Instance.MusicVolume, false);

        highestScoreText.text = FileManager.Instance.Score.ToString();
    }

    private void ShowSettings() {
        if (GameManager.Instance.IsGameRunning) {
            settingsScreen.SetActive(true);
            GameManager.Instance.SetIsGamePaused(true);
            PostProcessVolume processVolume = Camera.main.GetComponent<PostProcessVolume>();
            processVolume.enabled = true;
        }
    }

    public void HideSettings() {
        if (GameManager.Instance.IsGameRunning) {
            settingsScreen.SetActive(false);
            GameManager.Instance.SetIsGamePaused(false);
            PostProcessVolume processVolume = Camera.main.GetComponent<PostProcessVolume>();
            processVolume.enabled = false;
        }

    }

    private void SetGameplayMusicVolume(float volume, bool save = true) {
        AudioManager.Instance.SetGameplayMusicVolume(volume);
        gameplayMusicSlider.value = volume;
        if (volume == 0.0f) {
            gameplayMusicSpeakerImage.sprite = noteOff;
        } else {
            gameplayMusicSpeakerImage.sprite = noteOn;
            lastGameplayMusicVolume = volume;
        }
        if (save)
            FileManager.Instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume, bool save = true) {
        AudioManager.Instance.SetSFXVolume(volume);
        sfxSlider.value = volume;
        if (volume == 0.0f) {
            sfxSpeakerImage.sprite = speakerOff;
        } else {
            sfxSpeakerImage.sprite = speakerOn;
            lastSFXVolume = volume;
        }
        if (save)
            FileManager.Instance.SetSfxVolume(volume);
    }

    private void ShowPrivacyPolicy() {
        Application.OpenURL(url);
    }
}
