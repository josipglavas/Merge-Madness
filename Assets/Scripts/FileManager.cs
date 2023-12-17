using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class FileManager : MonoBehaviour {
    public static FileManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    public bool AdsRemoved { get; private set; } = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadFile();
    }

    public void SaveFile() {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);
        Debug.Log("File saved");
        GameData data = new GameData(Score, AdsRemoved);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public void LoadFile() {
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

        Debug.Log(data.highScore);
        Debug.Log(data.removeAds);
    }

    public void SetScore(int newScore) {
        Score = newScore;
        SaveFile();
    }

    public void SetRemoveAds(bool newRemoveAds) {
        AdsRemoved = newRemoveAds;
        SaveFile();
    }
}
