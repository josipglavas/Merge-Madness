using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

// iOS support package
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
using Apple.GameKit;
#endif

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    private string m_GooglePlayGamesToken;
    public Action PlayerSignedIn;
        
    private void Awake()
    {
#if UNITY_IOS
        ATTrackingStatusBinding.RequestAuthorizationTracking();
#endif

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
#endif

        // Setup subscription for Unity Player Accounts
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            UnitySignInSubscription();
        }
        else
        {
            UnityServices.Initialized += UnitySignInSubscription;
        }
    }

    private void UnitySignInSubscription()
    {
        UnityServices.Initialized -= UnitySignInSubscription;
        PlayerAccountService.Instance.SignedIn += SignInOrLinkWithUnity;
    }

    private async void Start()
    {
        Debug.Log("=== LoginManager Start() ===");

        // FIX 1: Wait for your OTHER script to finish initializing Unity Services
        // If we don't wait, AuthenticationService.Instance will throw errors.
        float timeout = 5f;
        while (UnityServices.State != ServicesInitializationState.Initialized && timeout > 0)
        {
            await Task.Delay(100);
            timeout -= 0.1f;
        }

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogError("Unity Services failed to initialize in time.");
            return;
        }

#if UNITY_EDITOR
        await SignInAnonymouslyAsync();
        return;
#endif

        // Step 1: Sign in anonymously (gets the session started)
        await SignInAnonymouslyAsync();

       // Step 2: Background link with Game Center or Google Play
#if UNITY_IOS
        Debug.Log("Attempting background Game Center link...");
        await LinkWithGameKitAsync();
#endif

#if UNITY_ANDROID
        StartSignInWithGooglePlayGames();
#endif
    }

    #region Apple Game Center (Unity 6 / Apple Plugin Fix)
#if UNITY_IOS
    private TaskCompletionSource<bool> m_GameCenterAuthTcs;

    private async Task LinkWithGameKitAsync()
    {
        try
        {
            Debug.Log("[GameKit] Starting link process...");

            // 1. Check if already authenticated to skip the wait
            if (GKLocalPlayer.Local.IsAuthenticated)
            {
                Debug.Log("[GameKit] Local player is already authenticated.");
            }
            else
            {
                m_GameCenterAuthTcs = new TaskCompletionSource<bool>();

                // 2. Subscribe to the static events in your GKLocalPlayer.cs
                // Note: We use the full path to avoid any confusion with namespaces
                GKLocalPlayer.AuthenticateUpdate += OnGKAuthenticateUpdate;
                GKLocalPlayer.AuthenticateError += OnGKAuthenticateError;

                Debug.Log("[GameKit] Calling Authenticate()...");
                
                // Fire and forget the call because we are listening to the events
                _ = GKLocalPlayer.Authenticate(); 

                // 3. Wait for the event listener OR a 30-second timeout
                var completedTask = await Task.WhenAny(m_GameCenterAuthTcs.Task, Task.Delay(30000));
                
                if (completedTask != m_GameCenterAuthTcs.Task)
                {
                    Debug.LogError("[GameKit] Auth timed out. Is Game Center enabled in Xcode?");
                    CleanupGKEvents();
                    return;
                }
                
                CleanupGKEvents();
            }

            // 4. Fetch the credentials and Link
            await FetchAndLinkGameCenter();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameKit] Exception: {ex.Message}");
            CleanupGKEvents();
        }
    }

    private void OnGKAuthenticateUpdate(GKLocalPlayer player)
    {
        Debug.Log($"[GameKit] Event Triggered: Authenticated as {player.DisplayName}");
        m_GameCenterAuthTcs?.TrySetResult(true);
    }

    // FIX: Match the signature to what Apple.Core provides
    private void OnGKAuthenticateError(Apple.Core.Runtime.NSError error)
    {
        string errorMsg = error != null ? error.LocalizedDescription : "Unknown GameKit Error";
        Debug.LogError($"[GameKit] Event Triggered Error: {errorMsg}");
        m_GameCenterAuthTcs?.TrySetException(new Exception(errorMsg));
    }

    private void CleanupGKEvents()
    {
        GKLocalPlayer.AuthenticateUpdate -= OnGKAuthenticateUpdate;
        GKLocalPlayer.AuthenticateError -= OnGKAuthenticateError;
    }

    private async Task FetchAndLinkGameCenter()
    {
        Debug.Log("[GameKit] Fetching Items for Verification...");
        
        // This uses the method confirmed in your provided source
        var fetchItemsResponse = await GKLocalPlayer.Local.FetchItemsForIdentityVerificationSignature();

        if (fetchItemsResponse == null)
        {
            Debug.LogError("[GameKit] Failed to fetch identity items.");
            return;
        }

        string signature = Convert.ToBase64String(fetchItemsResponse.GetSignature());
        string salt = Convert.ToBase64String(fetchItemsResponse.GetSalt());
        string publicKeyUrl = fetchItemsResponse.PublicKeyUrl;
        ulong timestamp = fetchItemsResponse.Timestamp;
        string teamPlayerID = GKLocalPlayer.Local.TeamPlayerId;
        string playerName = GKLocalPlayer.Local.DisplayName;
        
        if (!String.IsNullOrEmpty(playerName))
        {
            playerNameText.text = playerName; 
        }
        else
        {
            playerNameText.text = "";
        }
        
        await LinkWithAppleGameCenterAsync(signature, teamPlayerID, publicKeyUrl, salt, timestamp);
    }

    private async Task LinkWithAppleGameCenterAsync(string signature, string teamPlayerId, string publicKeyURL, string salt, ulong timestamp)
    {
        try
        {
            Debug.Log("[GameKit] Linking to Unity Authentication...");
            await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp);
            Debug.Log("[GameKit] Link successful!");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogWarning("[GameKit] Account already linked elsewhere. Signing in instead...");
            await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyURL, salt, timestamp);
            PlayerSignedIn?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameKit] Final Link Error: {ex.Message}");
        }
    }
#endif
#endregion

    #region Anonymous Sign In
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            // UGS handles session tokens automatically
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            PlayerSignedIn?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Unity Player Accounts
    public async void StartUnitySignInAsync()
    {
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            SignInOrLinkWithUnity();
            return;
        }

        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void SignInOrLinkWithUnity()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                PlayerSignedIn?.Invoke();
                return;
            }

            if (AuthenticationService.Instance.PlayerInfo.GetUnityId() == null)
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Linked Unity Account.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Google Play Games
#if UNITY_ANDROID
    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                });
            }
        });
    }

    public void StartSignInWithGooglePlayGames()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            LoginGooglePlayGames();
            return;
        }
        SignInOrLinkWithGooglePlayGames();
    }

    private async void SignInOrLinkWithGooglePlayGames()
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken)) return;

        try 
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
            else
                await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
        }
        catch (Exception ex) { Debug.LogException(ex); }
    }
#endif
    #endregion

    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        PlayerAccountService.Instance.SignOut();
        AuthenticationService.Instance.ClearSessionToken();
    }

    private void OnDestroy()
    {
        if (PlayerAccountService.Instance != null)
            PlayerAccountService.Instance.SignedIn -= SignInOrLinkWithUnity;
    }
}