using System.Collections;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UnityConsent;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LoginManager loginManager;
    [SerializeField] private Image loadingBackground;
    [SerializeField] private  float fadeDuration = 1f;

    private async void Awake()
    {
        loadingBackground.gameObject.SetActive(true);
        
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
           await UnityServices.InitializeAsync();
        }
        
        
        EndUserConsent.SetConsentState(new ConsentState {
            AnalyticsIntent = ConsentStatus.Granted,
            //AdsIntent = ConsentStatus.Denied
        });
    }

    private void Start()
    {
        AudioManager.Instance.StopGameplayMusic();
        PlayerSignedIn();
        // loginManager.PlayerSignedIn += PlayerSignedIn;
    }

    private void PlayerSignedIn()
    {
        AudioManager.Instance.StartGamePlayMusic();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.25f);
        float elapsed = 0.1f;
        Color color = loadingBackground.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            loadingBackground.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Ensure fully transparent
        loadingBackground.color = new Color(color.r, color.g, color.b, 0f);

        // Disable after fade
        loadingBackground.gameObject.SetActive(false);
    }
}
