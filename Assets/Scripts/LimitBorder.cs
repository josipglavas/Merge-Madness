using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitBorder : MonoBehaviour {
    public float timer = 4.5f;
    private bool startTimer = false;
    private GameObject lastCollidedObject = null;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Ornament") && collision.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic && !startTimer) {
            startTimer = true;
            lastCollidedObject = collision.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.CompareTag("Ornament") && collision.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic && !startTimer) {
            lastCollidedObject = collision.gameObject;
            startTimer = true;
        }
    }

    private void Update() {
        if (startTimer && !GameManager.Instance.IsGamePaused) {
            timer -= Time.deltaTime;
            if (timer < 4f) {
                UIController.Instance.ShowTimer(((int)timer).ToString());
                InputManager.Instance.SetCanUseInput(false);
            }
            if (timer <= 0f) {
                timer = 0f;
                TimerFinished();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject == lastCollidedObject) {
            if (UIController.Instance.IsTimerShown) {
                UIController.Instance.HideTimer();
            }
            if (timer < 4f) {
                InputManager.Instance.SetCanUseInput(true);
            }
            startTimer = false;
            timer = 4.5f;
        }
    }

    private void TimerFinished() {
        GameManager.Instance.GameOver();
    }
}