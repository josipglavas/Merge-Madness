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
        LoadInterstitial();
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
    }

    private void SdkInitialized() {
        Debug.Log("Sdk initialized!");
        LoadBanner();

    }
    private void OnApplicationPause(bool isPaused) {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    #region banner
    public void LoadBanner() {
        Debug.Log("Banner loaded");
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
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
        yield return new WaitForSeconds(10f);
        DestroyBanner();
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    }

    #endregion

    #region interstitial

    public void LoadInterstitial() {
        IronSource.Agent.loadInterstitial();
    }
    public void ShowInterstitial() {
        if (IronSource.Agent.isInterstitialReady()) {
            IronSource.Agent.showInterstitial();
        } else {
            Debug.Log("interstitial not ready!");
        }
    }


    /************* Interstitial AdInfo Delegates *************/
    // Invoked when the interstitial ad was loaded succesfully.
    private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {
        ShowInterstitial();
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

}