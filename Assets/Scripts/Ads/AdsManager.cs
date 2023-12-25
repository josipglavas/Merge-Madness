using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour {
#if UNITY_ANDROID
       private string appKey = "1ce010375";
#elif UNITY_IOS
    private string appKey = "1cdff70bd";
#else
    private string appKey = "unexpected_platform";
#endif

    private void Start() {
        IronSource.Agent.validateIntegration();
        //IronSource.Agent.init(appKey);
        GameManager.Instance.OnIsGameRunningChanged += GameManager_OnIsGameRunningChanged;
    }

    private void GameManager_OnIsGameRunningChanged(object sender, System.EventArgs e) {
        ShowInterstitial();
    }

    private void OnEnable() {
        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitialized;

        //Add AdInfo Banner Events
        IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

        //Add AdInfo Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    }

    private void OnDisable() {
        IronSourceEvents.onSdkInitializationCompletedEvent -= SdkInitialized;

        //Add AdInfo Banner Events
        IronSourceBannerEvents.onAdLoadedEvent -= BannerOnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent -= BannerOnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent -= BannerOnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent -= BannerOnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent -= BannerOnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent -= BannerOnAdLeftApplicationEvent;

        //Add AdInfo Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent -= InterstitialOnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent -= InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdOpenedEvent -= InterstitialOnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdClickedEvent -= InterstitialOnAdClickedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent -= InterstitialOnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent -= InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent -= InterstitialOnAdClosedEvent;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
    }

    private void SdkInitialized() {
        Debug.Log("Sdk initialized!");
        LoadBanner();
        LoadInterstitial();
        LoadRewarded();
    }
    private void OnApplicationPause(bool isPaused) {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    #region banner
    public void LoadBanner() {
        Debug.Log("Banner loaded");
        IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, IronSourceBannerPosition.BOTTOM);
    }
    public void DestroyBanner() {
        IronSource.Agent.destroyBanner();
    }


    /************* Banner AdInfo Delegates *************/
    //Invoked once the banner has loaded
    private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo) {
    }
    //Invoked when the banner loading process has failed.
    private void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError) {
        Debug.Log($"Banner failed to load with error: {ironSourceError}");
        StartCoroutine(RetryLoadingBannerAd());
    }
    // Invoked when end user clicks on the banner ad
    private void BannerOnAdClickedEvent(IronSourceAdInfo adInfo) {
    }
    //Notifies the presentation of a full screen content following user click
    private void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo) {
    }
    //Notifies the presented screen has been dismissed
    private void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo) {
    }
    //Invoked when the user leaves the app
    private void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo) {
    }

    private IEnumerator RetryLoadingBannerAd() {
        yield return new WaitForSeconds(21f);
        DestroyBanner();
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    }

    #endregion

    #region interstitial

    private void LoadInterstitial() {
        IronSource.Agent.loadInterstitial();
    }
    private void ShowInterstitial() {
        if (IronSource.Agent.isInterstitialReady()) {
            IronSource.Agent.showInterstitial();
        } else {
            Debug.Log("interstitial not ready!");
        }
    }


    /************* Interstitial AdInfo Delegates *************/
    // Invoked when the interstitial ad was loaded succesfully.
    private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {

    }
    // Invoked when the initialization process has failed.
    private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) {
        Debug.Log($"interstitial failed to load with error: {ironSourceError}");
    }
    // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo) {
    }
    // Invoked when end user clicked on the interstitial ad
    private void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) {
    }
    // Invoked when the ad failed to show.
    private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) {
    }
    // Invoked when the interstitial ad closed and the user went back to the application screen.
    private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) {
    }
    // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    // This callback is not supported by all networks, and we recommend using it only if  
    // it's supported by all networks you included in your build. 
    private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo) {
    }

    #endregion
    #region rewarded

    private void LoadRewarded() {
#if DEBUG
        //GameManager.Instance.SpawnUniversalObject();
#endif
        IronSource.Agent.loadRewardedVideo();
        Debug.Log("rewarded ad ready!");
    }

    public void ShowRewarded() {
        if (IronSource.Agent.isRewardedVideoAvailable()) {
            IronSource.Agent.showRewardedVideo();
            GameManager.Instance.SetIsGamePaused(true);
            Debug.Log("rewarded ad shown!");
        } else {
            Debug.Log("rewarded ad not ready!");
        }
    }


    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    private void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo) {
    }
    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    private void RewardedVideoOnAdUnavailable() {
        GameManager.Instance.SetIsGamePaused(false);
    }
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo) {
    }
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo) {
        GameManager.Instance.SetIsGamePaused(false);
    }
    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
        //create universal object
        GameManager.Instance.SpawnUniversalObject();
        GameManager.Instance.SetIsGamePaused(false);
    }
    // The rewarded video ad was failed to show.
    private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo) {
        GameManager.Instance.SetIsGamePaused(false);
    }
    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    private void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo) {
    }

    #endregion
}
