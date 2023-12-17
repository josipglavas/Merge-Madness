using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ornament List SO")]
public class OrnamentListSO : ScriptableObject {
    public List<OrnamentInfo> OrnamentList;

}
[System.Serializable]
public class OrnamentInfo {
    public GameObject prefab;
    public int score;

    public OrnamentInfo(GameObject prefab, int score) {
        this.prefab = prefab;
        this.score = score;
    }
}