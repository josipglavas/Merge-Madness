using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ornament : MonoBehaviour {

    public int ornamentSize;

    private void OnCollisionEnter2D(Collision2D collision) {
        Merge(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        Merge(collision);
    }


    private void Merge(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Ornament ornament) && ornament.ornamentSize == ornamentSize) {
            GameManager.Instance.ConnectOrnaments(this, ornament);
        }
    }

}
