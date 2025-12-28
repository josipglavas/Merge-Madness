using System;
using System.Collections;
using UnityEngine;

public class MainMenuCanvasManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuCanvasBackground;
    [SerializeField] private GameObject settingsCanvas;
    
    public bool IsShowingMainMenu { get; private set; }
    
    private IEnumerator Start()
    {
        yield return null;
        GameManager.Instance.SetIsGamePaused(true);
        IsShowingMainMenu = true;
        _mainMenuCanvasBackground.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        GameManager.Instance.SetIsGamePaused(false);
        IsShowingMainMenu = false;
        _mainMenuCanvasBackground.gameObject.SetActive(false);
    }

    public void ShowSettings()
    {
        _mainMenuCanvasBackground.gameObject.SetActive(true);
        settingsCanvas.SetActive(true);
    }
}
