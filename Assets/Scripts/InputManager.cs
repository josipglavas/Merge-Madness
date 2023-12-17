using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour {

    public static InputManager Instance { get; private set; }

    public event EventHandler<bool> OnCanUseInputChanged;

    private TouchControls touchControls;
    public float TouchXPos = 1;
    public bool TouchStarted = false;
    public bool TouchReleased = false;
    private Vector2 touchPosCam = Vector2.zero;
    private float maxTouchPosY = 3.435f;
    private bool canUseInput = true;
    private void Awake() {
        Instance = this;
        touchControls = new TouchControls();
    }

    private void OnEnable() {
        touchControls.Enable();
    }

    private void OnDisable() {
        touchControls.Disable();
    }

    private void Start() {
        touchControls.Touch.TouchPos.performed += TouchPos_performed;
        touchControls.Touch.Touching.started += Touching_started;
        touchControls.Touch.Touching.canceled += Touching_canceled;
    }

    private void Touching_started(InputAction.CallbackContext obj) {
        if (GameManager.Instance.CanPlace && !GameManager.Instance.IsGamePaused && touchPosCam.y < maxTouchPosY && canUseInput)
            TouchStarted = obj.ReadValueAsButton();
    }

    private void Touching_canceled(InputAction.CallbackContext obj) {
        if (GameManager.Instance.CanPlace && !GameManager.Instance.IsGamePaused && touchPosCam.y < maxTouchPosY && canUseInput) {
            TouchReleased = !obj.ReadValueAsButton();
            TouchStarted = false;
        }

    }

    private void TouchPos_performed(InputAction.CallbackContext obj) {
        if (!GameManager.Instance.IsGamePaused && canUseInput) {
            Vector2 touchPos = obj.ReadValue<Vector2>();
            if (Mathf.Abs(touchPos.x) < 1000000 && Mathf.Abs(touchPos.y) < 1000000)
                touchPosCam = Camera.main.ScreenToWorldPoint(touchPos);
            if (touchPosCam.y < maxTouchPosY) {
                TouchXPos = touchPos.x;
            }
        }
    }

    public void SetCanUseInput(bool canUseInput) {
        this.canUseInput = canUseInput;
        OnCanUseInputChanged?.Invoke(this, this.canUseInput);
    }
}
