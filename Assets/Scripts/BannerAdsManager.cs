using UnityEngine;
using Unity.Services.LevelPlay;
using GoogleMobileAds.Api;

/// <summary>
/// Banner Ads Manager
/// Priority:
/// 1. Unity LevelPlay (ironSource)
/// 2. Google AdMob fallback after 3 failures
/// </summary>
public class BannerAdsManager : MonoBehaviour
{
    // --------------------------------------------------------------------
    // LevelPlay Configuration
    // --------------------------------------------------------------------

    private const string ANDROID_APP_KEY = "";
    private const string IOS_APP_KEY = "1cdff70bd";

    [Header("LevelPlay Banner")]
    [SerializeField] private string levelPlayBannerAdUnitId = "6vhjdtzea40y22xp";

    // --------------------------------------------------------------------
    // AdMob Configuration (Platform Specific)
    // --------------------------------------------------------------------

    [Header("AdMob Banner")]
    [SerializeField] private string admobBannerAndroidId = "ca-app-pub-4846032734396794/8109113206";
    [SerializeField] private string admobBannerIosId = "ca-app-pub-4846032734396794/3077619705";

    private BannerView admobBannerView;

    // --------------------------------------------------------------------
    // Runtime State
    // --------------------------------------------------------------------

    private LevelPlayBannerAd levelPlayBanner;
    private bool sdkInitialized;
    private int levelPlayRetryCount;
    private const int MAX_LEVELPLAY_RETRIES = 3;

    // --------------------------------------------------------------------
    // Initialization
    // --------------------------------------------------------------------

    private void Start()
    {
#if UNITY_EDITOR
       
        CreateAdMobBanner();
        return;
#endif
        RegisterLevelPlayEvents();
        InitializeLevelPlay();
    }

    private void InitializeLevelPlay()
    {
        if (sdkInitialized)
            return;

        LevelPlay.Init(GetPlatformAppKey(), null);
    }

    private string GetPlatformAppKey()
    {
#if UNITY_ANDROID
        return ANDROID_APP_KEY;
#elif UNITY_IOS
        return IOS_APP_KEY;
#else
        return string.Empty;
#endif
    }

    private void RegisterLevelPlayEvents()
    {
        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        LevelPlay.OnInitFailed += OnLevelPlayInitFailed;
    }

    // --------------------------------------------------------------------
    // LevelPlay Callbacks
    // --------------------------------------------------------------------

    private void OnLevelPlayInitialized(LevelPlayConfiguration config)
    {
        Debug.Log("[BannerAds] LevelPlay initialized.");
        sdkInitialized = true;

        CreateLevelPlayBanner();
        LoadLevelPlayBanner();
    }

    private void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"[BannerAds] LevelPlay init failed: {error.ErrorMessage}");
        CreateAdMobBanner();
    }

    // --------------------------------------------------------------------
    // LevelPlay Banner
    // --------------------------------------------------------------------

    private void CreateLevelPlayBanner()
    {
        levelPlayBanner = new LevelPlayBannerAd(levelPlayBannerAdUnitId);

        levelPlayBanner.OnAdLoaded += OnLevelPlayBannerLoaded;
        levelPlayBanner.OnAdLoadFailed += OnLevelPlayBannerFailed;
    }

    private void LoadLevelPlayBanner()
    {
        if (levelPlayBanner == null)
            return;

        Debug.Log("[BannerAds] Loading LevelPlay banner...");
        levelPlayBanner.LoadAd();
    }

    private void OnLevelPlayBannerLoaded(LevelPlayAdInfo ad)
    {
        Debug.Log("[BannerAds] LevelPlay banner loaded.");
        levelPlayRetryCount = 0;
        levelPlayBanner.ShowAd();
    }

    private void OnLevelPlayBannerFailed(LevelPlayAdError error)
    {
        levelPlayRetryCount++;

        Debug.LogError(
            $"[BannerAds] LevelPlay banner failed ({levelPlayRetryCount}/{MAX_LEVELPLAY_RETRIES})"
        );

        if (levelPlayRetryCount >= MAX_LEVELPLAY_RETRIES)
        {
            Debug.Log("[BannerAds] Switching to AdMob banner.");
            DestroyLevelPlayBanner();
            CreateAdMobBanner();
        }
        else
        {
            Invoke(nameof(LoadLevelPlayBanner), 1f);
        }
    }

    private void DestroyLevelPlayBanner()
    {
        if (levelPlayBanner == null)
            return;

        levelPlayBanner.DestroyAd();
        levelPlayBanner = null;
    }

    // --------------------------------------------------------------------
    // AdMob Banner (Fallback)
    // --------------------------------------------------------------------

    private void CreateAdMobBanner()
    {
        if (admobBannerView != null)
            return;

        string adUnitId = GetAdMobBannerId();

        if (string.IsNullOrEmpty(adUnitId))
        {
            Debug.LogError("[BannerAds] AdMob Banner ID missing for this platform.");
            return;
        }

        Debug.Log("[BannerAds] Creating AdMob banner.");

        MobileAds.Initialize(_ => { });

        admobBannerView = new BannerView(
            adUnitId,
            AdSize.Banner,
            AdPosition.Bottom
        );

        AdRequest request = new AdRequest();
        admobBannerView.LoadAd(request);
    }

    private string GetAdMobBannerId()
    {
#if UNITY_ANDROID
        return admobBannerAndroidId;
#elif UNITY_IOS
        return admobBannerIosId;
#else
        return string.Empty;
#endif
    }

    // --------------------------------------------------------------------
    // Cleanup
    // --------------------------------------------------------------------

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;

        DestroyLevelPlayBanner();

        if (admobBannerView != null)
        {
            admobBannerView.Destroy();
            admobBannerView = null;
        }
    }
}
