[System.Serializable]
public class GameData {
    public int highScore;
    public bool removeAds;
    public float sfxVolume;
    public float musicVolume;
    public GameData(int scoreInt, bool removeAdsBool, float sfxVolumeFloat, float musicVolumeFloat) {
        highScore = scoreInt;
        removeAds = removeAdsBool;
        sfxVolume = sfxVolumeFloat;
        musicVolume = musicVolumeFloat;
    }
}