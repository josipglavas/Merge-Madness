using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class GameCenterManager : MonoBehaviour {

    public static GameCenterManager Instance { get; private set; }

    [SerializeField] private Button showLeaderBoardButton;

    private void Awake() {
        Instance = this;

        showLeaderBoardButton.onClick.AddListener(() => {
            Social.ShowLeaderboardUI();
        });
    }

    private void Start() {
        // Authenticate
        Social.localUser.Authenticate(ProcessAuthentication);

    }

    private void ProcessAuthentication(bool success) {
        if (success) {
            Debug.Log("Authenticated, checking achievements");
        } else {
            Debug.Log("Failed to authenticate");
        }
    }

    public void ReportScore(int score) {
        string leaderboardID = "XmasHighScore";
        Debug.Log("Reporting score " + score + " on leaderboard " + leaderboardID);
        Social.ReportScore(score, leaderboardID, success => {
            Debug.Log(success ? "Reported score successfully" : "Failed to report score");
        });
    }

    //sendScoreButton.GetComponent<Button>().onClick.AddListener(() => { ReportScore(sliderVal, "1"); });
    //checkScoreboardButton.GetComponent<Button>().onClick.AddListener(() => { Social.ShowLeaderboardUI(); });
}
