using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI GameOverScoreText;
    [SerializeField] private GameObject spawnPositionPointer;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private GameObject timer;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverOverlay;
    [SerializeField] private Image nextOrnamentImage;

    [SerializeField] private UISettings uISettings;

    public bool IsTimerShown = false;

    private void Awake()
    {
        Instance = this;

        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;

        // 🔔 SUBSCRIBE TO NEXT ORNAMENT EVENT
        GameManager.Instance.OnNextOrnamentChanged += UpdateNextOrnamentImage;

        playAgainButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    private void Start()
    {
        gameOverOverlay.SetActive(false);
        spawnPositionPointer.SetActive(false);
        uISettings.OnAssistedModeChanged += UISettings_OnAssistedModeChanged;
        nextOrnamentImage.transform.parent.gameObject.SetActive(uISettings.IsAssistedModeOn());
    }

    private void UISettings_OnAssistedModeChanged(object sender, bool isOn)
    {
        nextOrnamentImage.transform.parent.gameObject.SetActive(isOn);
    }

    private void UpdateNextOrnamentImage(Sprite sprite)
    {
        nextOrnamentImage.sprite = sprite;
        nextOrnamentImage.enabled = sprite != null;
    }

    private void GameManager_OnScoreChanged(object sender, int totalScore)
    {
        scoreText.text = totalScore.ToString();
        GameOverScoreText.text = totalScore.ToString();
    }

    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e)
    {
        PostProcessVolume processVolume = Camera.main.GetComponent<PostProcessVolume>();
        processVolume.enabled = true;
        gameOverOverlay.SetActive(true);
    }

    public void ShowTimer(string time)
    {
        IsTimerShown = true;
        timer.SetActive(true);
        timerText.text = time;
    }

    public void HideTimer()
    {
        if (timer != null)
            timer.SetActive(false);
        IsTimerShown = false;
    }
}
