using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class UIController : MonoBehaviour {

    public static UIController Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI GameOverScoreText;
    [SerializeField] private GameObject spawnPositionPointer;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private GameObject timer;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverOverlay;

    public bool IsTimerShown = false;

    private void Awake() {
        Instance = this;

        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;

        playAgainButton.onClick.AddListener(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name.ToString());
        });

    }

    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e) {
        PostProcessVolume processVolume = Camera.main.GetComponent<PostProcessVolume>();
        processVolume.enabled = true;
        gameOverOverlay.SetActive(true);
    }

    private void Start() {
        gameOverOverlay.SetActive(false);
        spawnPositionPointer.SetActive(false);
    }

    private void GameManager_OnScoreChanged(object sender, int totalScore) {
        scoreText.text = totalScore.ToString();
        GameOverScoreText.text = totalScore.ToString();
    }

    public void ShowTimer(string time) {
        IsTimerShown = true;
        timer.SetActive(true);
        timerText.text = time;
    }

    public void HideTimer() {
        if (timer != null)
            timer.SetActive(false);
        IsTimerShown = false;
    }


}
