using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour {

    [SerializeField] private string bannerId = "ca-app-pub-4846032734396794/8109113206";
    [SerializeField] private string interId = "ca-app-pub-4846032734396794/4277663493";
    [SerializeField] private string rewardedId = "ca-app-pub-4846032734396794/2964581827";

    [SerializeField] private Button rewardAdButton;

    private enum AdType {
        Banner,
        Interstitial,
        Rewarded,
    }

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private void Awake() {
        rewardAdButton.onClick.AddListener(() => {
            ShowRewardedAd();
        });
    }
    private void Start() {
        rewardAdButton.gameObject.SetActive(false);

        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus => {

            print("Ads Initialised!");
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });

        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;
    }

    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e) {
        ShowInterstitialAd();
    }

    private IEnumerator RetryLoadingAd(AdType adType) {
        yield return new WaitForSeconds(5f);
        switch (adType) {
            case AdType.Banner:
                DestroyBannerAd();
                LoadBannerAd();
                break;
            case AdType.Interstitial:
                LoadInterstitialAd();
                break;
            case AdType.Rewarded:
                LoadRewardedAd();
                break;
        }
    }

    #region Banner

    private void LoadBannerAd() {
        //create a banner
        CreateBannerView();

        //listen to banner events
        ListenToBannerEvents();

        //load the banner
        if (bannerView == null) {
            CreateBannerView();
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        print("Loading banner Ad !!");
        bannerView.LoadAd(adRequest);//show the banner on the screen
    }
    private void CreateBannerView() {

        if (bannerView != null) {
            DestroyBannerAd();
        }
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
    }
    private void ListenToBannerEvents() {
        bannerView.OnBannerAdLoaded += () => {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) => {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
            StartCoroutine(RetryLoadingAd(AdType.Banner));
        };
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) => {
            Debug.Log("Banner view paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () => {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () => {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () => {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () => {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    private void DestroyBannerAd() {

        if (bannerView != null) {
            print("Destroying banner Ad");
            bannerView.Destroy();
            bannerView = null;
        }
    }
    #endregion

    #region Interstitial

    private void LoadInterstitialAd() {

        if (interstitialAd != null) {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) => {
            if (error != null || ad == null) {
                print("Interstitial ad failed to load" + error);
                StartCoroutine(RetryLoadingAd(AdType.Interstitial));
                return;
            }

            print("Interstitial ad loaded !!" + ad.GetResponseInfo());

            interstitialAd = ad;
            InterstitialEvent(interstitialAd);
        });

    }
    private void ShowInterstitialAd() {

        if (interstitialAd != null && interstitialAd.CanShowAd()) {
            interstitialAd.Show();
        } else {
            print("Intersititial ad not ready!!");
        }
    }
    private void InterstitialEvent(InterstitialAd ad) {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => {
            Debug.Log("Interstitial ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () => {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("Interstitial ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    #endregion

    #region Rewarded

    private void LoadRewardedAd() {
        if (rewardedAd != null) {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) => {
            if (error != null || ad == null) {
                print("Rewarded failed to load" + error);
                rewardAdButton.gameObject.SetActive(false);
                StartCoroutine(RetryLoadingAd(AdType.Rewarded));
                return;
            }
            print("Rewarded ad loaded !!");
            rewardAdButton.gameObject.SetActive(true);
            rewardedAd = ad;
            RewardedAdEvents(rewardedAd);
        });
    }
    private void ShowRewardedAd() {
        GameManager.Instance.SetIsGamePaused(true);
        if (rewardedAd != null && rewardedAd.CanShowAd()) {
            rewardedAd.Show((Reward reward) => {
                print("Give reward to player !!");
                GameManager.Instance.SpawnUniversalObject();
                GameManager.Instance.SetIsGamePaused(false);
            });
        } else {
            print("Rewarded ad not ready");
            GameManager.Instance.SetIsGamePaused(false);
            LoadRewardedAd();
        }
    }
    private void RewardedAdEvents(RewardedAd ad) {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => {
            Debug.Log("Rewarded ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () => {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () => {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () => {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    #endregion

    //#if UNITY_ANDROID
    //       private string appKey = "1ce010375";
    //#elif UNITY_IOS
    //    private string appKey = "1cdff70bd";
    //#else
    //    private string appKey = "unexpected_platform";
    //#endif

    //    [SerializeField] private Button rewardAdButton;

    //    private enum AdType {
    //        Banner,
    //        Interstitial,
    //        Rewarded,
    //    }

    //    private void Awake() {
    //        rewardAdButton.onClick.AddListener(() => {
    //            ShowRewarded();
    //        });
    //    }

    //    private void Start() {
    //        IronSource.Agent.validateIntegration();
    //        //IronSource.Agent.init(appKey);
    //        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;
    //        rewardAdButton.gameObject.SetActive(false);
    //    }

    //    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e) {
    //        ShowInterstitial();
    //    }

    //    private void OnEnable() {
    //        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitialized;

    //        //Add AdInfo Banner Events
    //        IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
    //        IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
    //        IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
    //        IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
    //        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
    //        IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

    //        //Add AdInfo Interstitial Events
    //        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
    //        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
    //        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
    //        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
    //        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
    //        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
    //        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

    //        //Add AdInfo Rewarded Video Events
    //        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
    //        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
    //        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
    //        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
    //        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
    //        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    //        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    //    }

    //    private void OnDisable() {
    //        IronSourceEvents.onSdkInitializationCompletedEvent -= SdkInitialized;

    //        //Add AdInfo Banner Events
    //        IronSourceBannerEvents.onAdLoadedEvent -= BannerOnAdLoadedEvent;
    //        IronSourceBannerEvents.onAdLoadFailedEvent -= BannerOnAdLoadFailedEvent;
    //        IronSourceBannerEvents.onAdClickedEvent -= BannerOnAdClickedEvent;
    //        IronSourceBannerEvents.onAdScreenPresentedEvent -= BannerOnAdScreenPresentedEvent;
    //        IronSourceBannerEvents.onAdScreenDismissedEvent -= BannerOnAdScreenDismissedEvent;
    //        IronSourceBannerEvents.onAdLeftApplicationEvent -= BannerOnAdLeftApplicationEvent;

    //        //Add AdInfo Interstitial Events
    //        IronSourceInterstitialEvents.onAdReadyEvent -= InterstitialOnAdReadyEvent;
    //        IronSourceInterstitialEvents.onAdLoadFailedEvent -= InterstitialOnAdLoadFailed;
    //        IronSourceInterstitialEvents.onAdOpenedEvent -= InterstitialOnAdOpenedEvent;
    //        IronSourceInterstitialEvents.onAdClickedEvent -= InterstitialOnAdClickedEvent;
    //        IronSourceInterstitialEvents.onAdShowSucceededEvent -= InterstitialOnAdShowSucceededEvent;
    //        IronSourceInterstitialEvents.onAdShowFailedEvent -= InterstitialOnAdShowFailedEvent;
    //        IronSourceInterstitialEvents.onAdClosedEvent -= InterstitialOnAdClosedEvent;

    //        //Add AdInfo Rewarded Video Events
    //        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
    //        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
    //        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
    //        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
    //        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
    //        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    //        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
    //    }

    //    private void SdkInitialized() {
    //        Debug.Log("Sdk initialized!");
    //        LoadBanner();
    //        LoadInterstitial();
    //        LoadRewarded();
    //    }
    //    private void OnApplicationPause(bool isPaused) {
    //        IronSource.Agent.onApplicationPause(isPaused);
    //    }

    //    #region banner
    //    public void LoadBanner() {
    //        Debug.Log("Banner loaded");
    //        IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
    //    }
    //    private void DestroyBanner() {
    //        IronSource.Agent.destroyBanner();
    //    }


    //    /************* Banner AdInfo Delegates *************/
    //    //Invoked once the banner has loaded
    //    private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    //Invoked when the banner loading process has failed.
    //    private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) {
    //        Debug.Log($"Banner failed to load with error: {ironSourceError}");
    //        StartCoroutine(RetryLoadingAd(AdType.Banner));
    //    }
    //    // Invoked when end user clicks on the banner ad
    //    private void BannerOnAdClickedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    //Notifies the presentation of a full screen content following user click
    //    private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    //Notifies the presented screen has been dismissed
    //    private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    //Invoked when the user leaves the app
    //    private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo) {
    //    }

    //    private IEnumerator RetryLoadingAd(AdType adType) {
    //        yield return new WaitForSeconds(5f);
    //        switch (adType) {
    //            case AdType.Banner:
    //                DestroyBanner();
    //                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    //                break;
    //            case AdType.Interstitial:
    //                IronSource.Agent.loadInterstitial();
    //                break;

    //            case AdType.Rewarded:
    //                IronSource.Agent.loadRewardedVideo();
    //                break;
    //        }
    //    }

    //    #endregion

    //    #region interstitial

    //    private void LoadInterstitial() {
    //        IronSource.Agent.loadInterstitial();
    //    }
    //    private void ShowInterstitial() {
    //        if (IronSource.Agent.isInterstitialReady()) {
    //            IronSource.Agent.showInterstitial();
    //        } else {
    //            Debug.Log("interstitial not ready!");
    //        }
    //    }


    //    /************* Interstitial AdInfo Delegates *************/
    //    // Invoked when the interstitial ad was loaded succesfully.
    //    private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {

    //    }
    //    // Invoked when the initialization process has failed.
    //    private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) {
    //        Debug.Log($"interstitial failed to load with error: {ironSourceError}");
    //        StartCoroutine(RetryLoadingAd(AdType.Interstitial));
    //    }
    //    // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    //    private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    // Invoked when end user clicked on the interstitial ad
    //    private void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    // Invoked when the ad failed to show.
    //    private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) {
    //    }
    //    // Invoked when the interstitial ad closed and the user went back to the application screen.
    //    private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    //    // This callback is not supported by all networks, and we recommend using it only if  
    //    // it's supported by all networks you included in your build. 
    //    private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo) {
    //    }

    //    #endregion
    //    #region rewarded

    //    private void LoadRewarded() {
    //#if DEBUG
    //        //GameManager.Instance.SpawnUniversalObject();
    //#endif
    //        IronSource.Agent.loadRewardedVideo();

    //        Debug.Log("rewarded ad ready!");
    //    }

    //    private void ShowRewarded() {
    //        if (IronSource.Agent.isRewardedVideoAvailable()) {
    //            IronSource.Agent.showRewardedVideo();
    //            GameManager.Instance.SetIsGamePaused(true);
    //            Debug.Log("rewarded ad shown!");
    //        } else {
    //            Debug.Log("rewarded ad not ready!");
    //        }
    //    }


    //    /************* RewardedVideo AdInfo Delegates *************/
    //    // Indicates that there’s an available ad.
    //    // The adInfo object includes information about the ad that was loaded successfully
    //    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    //    private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo) {
    //        rewardAdButton.gameObject.SetActive(true);
    //    }
    //    // Indicates that no ads are available to be displayed
    //    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    //    private void RewardedVideoOnAdUnavailable() {
    //        GameManager.Instance.SetIsGamePaused(false);
    //        rewardAdButton.gameObject.SetActive(false);
    //        StartCoroutine(RetryLoadingAd(AdType.Rewarded));
    //    }
    //    // The Rewarded Video ad view has opened. Your activity will loose focus.
    //    private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo) {
    //    }
    //    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    //    private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo) {
    //        GameManager.Instance.SetIsGamePaused(false);
    //    }
    //    // The user completed to watch the video, and should be rewarded.
    //    // The placement parameter will include the reward data.
    //    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    //    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
    //        //create universal object
    //        GameManager.Instance.SpawnUniversalObject();
    //        GameManager.Instance.SetIsGamePaused(false);
    //    }
    //    // The rewarded video ad was failed to show.
    //    private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo) {
    //        GameManager.Instance.SetIsGamePaused(false);
    //    }
    //    // Invoked when the video ad was clicked.
    //    // This callback is not supported by all networks, and we recommend using it only if
    //    // it’s supported by all networks you included in your build.
    //    private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
    //    }

    //    #endregion
}
