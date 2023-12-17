[System.Serializable]
public class GameData {
    public int highScore;
    public bool removeAds;
    public GameData(int scoreInt, bool removeAdsBool) {
        highScore = scoreInt;
        removeAds = removeAdsBool;
    }
}