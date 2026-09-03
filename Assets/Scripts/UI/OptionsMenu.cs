using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static event Action<bool> OnOptionsCanvasToggled;
    public static event Action OnRestartRequested;

    [SerializeField] Canvas _mainCanvas, _audioCanvas;
    [SerializeField] GameObject _unpauseButton, _mainMenuButton, _cancelQuitButton, _cancelRestartButton, _restartPrompt, _quitPrompt;
    [SerializeField] Toggle _invertYToggle;
    [SerializeField] Slider _lookSensitivitySlider;

    [SerializeField] CameraOptionsSO _cameraOptions;

    bool _sceneChanging;

    public static readonly string INVERT_Y = "InvertY", LOOK_SENSITIVITY = "LookSensitivity";

    void Awake()
    {
        InputManager.OnOptionsPressed += ToggleOptionsCanvas;
        LevelManager.OnSceneChangeStarted += LevelManager_OnScenChangeStarted;

        LoadCameraOptionValues();
    }

    void OnDestroy()
    {
        InputManager.OnOptionsPressed -= ToggleOptionsCanvas;
        LevelManager.OnSceneChangeStarted -= LevelManager_OnScenChangeStarted;
    }

    void LoadCameraOptionValues()
    {
        _invertYToggle.isOn = PlayerPrefs.GetInt(INVERT_Y, 1) == 1;
        SetInvertLookY();

        _lookSensitivitySlider.value = PlayerPrefs.GetFloat(LOOK_SENSITIVITY, 1);
        SetLookSensitivity(_lookSensitivitySlider.value);
    }

    public void SetInvertLookY()    // Hooked up to UI Toggle
    {
        if(_invertYToggle.isOn)
        {
            PlayerPrefs.SetInt(INVERT_Y, 1);
        }
        else
        {
            PlayerPrefs.SetInt(INVERT_Y, 0);
        }
        _cameraOptions.SetInvertY(_invertYToggle.isOn);
    }

    public void SetLookSensitivity(float sliderValue)
    {
        PlayerPrefs.SetFloat(LOOK_SENSITIVITY, sliderValue);
        _cameraOptions.SetSensitivity(sliderValue);
    }

    void ToggleOptionsCanvas()
    {
        if(_sceneChanging) { return; }

        if(_audioCanvas.enabled)
        {
            DisableAudioCanvas();
            return;
        }

        _mainCanvas.enabled = !_mainCanvas.enabled;

        if(_mainCanvas.enabled)
        {
            EnableOptionsCanvas();
        }
        else
        {
            DisableOptionsCanvas();
        }
    }

    public void EnableOptionsCanvas()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _mainCanvas.enabled = true;
        OnOptionsCanvasToggled?.Invoke(true);
        EventSystem.current.SetSelectedGameObject(_unpauseButton);
    }

    public void DisableOptionsCanvas()
    {
        _mainCanvas.enabled = false;
        EventSystem.current.SetSelectedGameObject(null);
        OnOptionsCanvasToggled?.Invoke(false);
    }

    public void EnableAudioCanvas()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _audioCanvas.enabled = true;
        EventSystem.current.SetSelectedGameObject(_mainMenuButton);
    }

    public void DisableAudioCanvas()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _audioCanvas.enabled = false;
        EventSystem.current.SetSelectedGameObject(_unpauseButton);
    }

    public void PromptRestart()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _restartPrompt.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_cancelRestartButton);
    }

    public void CancelRestart()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _restartPrompt.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_unpauseButton);
    }

    public void RestartLevel()
    {
        if(_sceneChanging) { return; }

        DisableOptionsCanvas();
        _sceneChanging = true;
        OnRestartRequested?.Invoke();
    }

    public void PromptQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _quitPrompt.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_cancelQuitButton);
    }

    public void CancelQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _quitPrompt.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_unpauseButton);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void LevelManager_OnScenChangeStarted()
    {
        _sceneChanging = true;
    }
}
