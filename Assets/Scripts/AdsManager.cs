using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;
using System.Collections;

public class AdsManager : MonoBehaviour
{
    
#if UNITY_ANDROID
    private const string APP_KEY = "1ce010375";
#elif UNITY_IOS
    private const string APP_KEY = "1cdff70bd";
#else
    private const string APP_KEY = "";
#endif

    // --------------------------------------------------------------------
    // Ad Unit IDs (COPIED FROM YOUR SCRIPTS)
    // --------------------------------------------------------------------

    [Header("Ad Unit IDs")]
    [SerializeField] private string interstitialAdUnitId = "gruponupujxrnzc3";
    [SerializeField] private string rewardedAdUnitId = "g85cndqkvqi5w8lv";
    [SerializeField] private string rewardedPlacement = "Level_Complete";

    [Header("UI")]
    [SerializeField] private Button rewardAdButton;

    // --------------------------------------------------------------------
    // Runtime
    // --------------------------------------------------------------------

    private bool isInitialized;

    private LevelPlayBannerAd banner;
    private LevelPlayInterstitialAd interstitial;
    private LevelPlayRewardedAd rewarded;

    // --------------------------------------------------------------------
    // Unity Lifecycle
    // --------------------------------------------------------------------

    private void Awake()
    {
        rewardAdButton.onClick.AddListener(ShowRewarded);
        rewardAdButton.gameObject.SetActive(false);
    }

    private void Start()
    {
#if UNITY_EDITOR
        Debug.Log("[EDITOR] Ads are simulated in editor.");
        return;
#endif

        LevelPlay.OnInitSuccess += OnSdkInitialized;
        LevelPlay.OnInitFailed += OnSdkInitFailed;

            InitializeSdk();

        GameManager.Instance.OnIsGameRunningChanged += OnGameRunningChanged;
    }

    private void InitializeSdk()
    {
        if (isInitialized) return;

        Debug.Log("[Ads] Initializing LevelPlay SDK");
        LevelPlay.Init(APP_KEY);
        // LevelPlay.SetPauseGame(true);
    }

    private void OnSdkInitialized(LevelPlayConfiguration config)
    {
        Debug.Log("[Ads] SDK Initialized");
        isInitialized = true;

        CreateInterstitial();
        CreateRewarded();

        LoadInterstitial();
        LoadRewarded();
    }

    private void OnSdkInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"[Ads] SDK Init Failed: {error.ErrorMessage}");
    }

    // --------------------------------------------------------------------
    // Interstitial
    // --------------------------------------------------------------------

    private void CreateInterstitial()
    {
        if (string.IsNullOrEmpty(interstitialAdUnitId))
            return;

        interstitial = new LevelPlayInterstitialAd(interstitialAdUnitId);
        interstitial.OnAdLoaded += _ => Debug.Log("Interstitial Loaded");
        interstitial.OnAdLoadFailed += _ => Retry(LoadInterstitial);
        interstitial.OnAdClosed += _ => LoadInterstitial();
    }

    public void showInterstitial()
    {
        interstitial?.ShowAd();
    }
    
    private void LoadInterstitial()
    {
        interstitial?.LoadAd();
    }

    private void ShowInterstitial()
    {
        if (interstitial != null && interstitial.IsAdReady())
            interstitial.ShowAd();
    }

    private void OnGameRunningChanged(object sender, System.EventArgs e)
    {
        ShowInterstitial();
    }

    // --------------------------------------------------------------------
    // Rewarded
    // --------------------------------------------------------------------

    private void CreateRewarded()
    {
        rewarded = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewarded.OnAdLoaded += _ =>
        {
            rewardAdButton.gameObject.SetActive(true);
            Debug.Log("Rewarded Ready");
        };

        rewarded.OnAdLoadFailed += _ =>
        {
            rewardAdButton.gameObject.SetActive(false);
            Retry(LoadRewarded);
        };

        rewarded.OnAdRewarded += (_, __) =>
        {
            GameManager.Instance.SpawnUniversalObject();
            GameManager.Instance.SetIsGamePaused(false);
        };

        rewarded.OnAdClosed += _ =>
        {
            GameManager.Instance.SetIsGamePaused(false);
            LoadRewarded();
        };
    }

    private void LoadRewarded()
    {
        rewarded?.LoadAd();
    }

    private void ShowRewarded()
    {
        if (rewarded != null && rewarded.IsAdReady())
        {
            GameManager.Instance.SetIsGamePaused(true);
            rewarded.ShowAd(rewardedPlacement);
        }
    }

    // --------------------------------------------------------------------
    // Utils
    // --------------------------------------------------------------------

    private void Retry(System.Action action)
    {
        StartCoroutine(RetryRoutine(action));
    }

    private IEnumerator RetryRoutine(System.Action action)
    {
        yield return new WaitForSeconds(3f);
        action?.Invoke();
    }

    // --------------------------------------------------------------------
    // Cleanup
    // --------------------------------------------------------------------

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnSdkInitialized;
        LevelPlay.OnInitFailed -= OnSdkInitFailed;
    }
}