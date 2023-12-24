using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class FileManager : MonoBehaviour {
    public static FileManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    public bool AdsRemoved { get; private set; } = false;
    public float MusicVolume { get; private set; } = 0.5f;
    public float SfxVolume { get; private set; } = 1.0f;

    private void Awake() {
        Instance = this;
        LoadFile();
    }

    private void SaveFile() {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        Debug.Log("File saved");
        GameData data = new GameData(Score, AdsRemoved, SfxVolume, MusicVolume);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    private void LoadFile() {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else {
            SaveFile();
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        GameData data = (GameData)bf.Deserialize(file);
        file.Close();

        Score = data.highScore;
        AdsRemoved = data.removeAds;
        SfxVolume = data.sfxVolume;
        MusicVolume = data.musicVolume;
    }

    public void SetScore(int newScore) {
        if (newScore > Score) {
            Score = newScore;
            SaveFile();
        }
    }

    public void SetRemoveAds(bool newRemoveAds) {
        AdsRemoved = newRemoveAds;
        SaveFile();
    }

    public void SetSfxVolume(float volume) {
        if (volume > 1.0f)
            return;
        SfxVolume = volume;
        Debug.Log(volume);
        SaveFile();
    }

    public void SetMusicVolume(float volume) {
        if (volume > 1.0f)
            return;
        Debug.Log(volume);
        MusicVolume = volume;
        SaveFile();
    }
}
