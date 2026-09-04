using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public static event Action<bool> OnOptionsCanvasToggled;
    public static event Action OnRestartRequested, OnCameraValuesChanged;

    [SerializeField] Canvas _mainCanvas, _audioCanvas;
    [SerializeField] GameObject _unpauseButton, _mainMenuButton, _cancelQuitButton, _cancelRestartButton, _restartPrompt, _quitPrompt;
    [SerializeField] Toggle _invertYToggle, _firstPersonToggle;
    [SerializeField] Slider _lookSensitivitySlider, _fovSlider, _cameraSideSlider;

    [SerializeField] CameraOptionsSO _cameraOptions;

    bool _sceneChanging;

    public static readonly string FIRST_PERSON = "FirstPerson", INVERT_Y = "InvertY", LOOK_SENSITIVITY = "LookSensitivity", CAMERA_FOV = "FieldOfView", CAMERA_SIDE = "CameraSide";

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
        _firstPersonToggle.isOn = PlayerPrefs.GetInt(FIRST_PERSON, 0) == 1;
        SetFirstPerson();

        _invertYToggle.isOn = PlayerPrefs.GetInt(INVERT_Y, 0) == 1;
        SetInvertLookY();

        _lookSensitivitySlider.value = PlayerPrefs.GetFloat(LOOK_SENSITIVITY, 1);
        SetLookSensitivity(_lookSensitivitySlider.value);

        _fovSlider.value = PlayerPrefs.GetFloat(CAMERA_FOV, 70f);
        SetCameraFOV(_fovSlider.value);

        _cameraSideSlider.value = PlayerPrefs.GetFloat(CAMERA_SIDE, 0.85f);
        SetCameraSideValue(_cameraSideSlider.value);

        OnCameraValuesChanged?.Invoke();
    }

    public void SetFirstPerson()
    {
        if(_firstPersonToggle.isOn)
        {
            PlayerPrefs.SetInt(FIRST_PERSON, 1);
        }
        else
        {
            PlayerPrefs.SetInt(FIRST_PERSON, 0);
        }
        _cameraOptions.SetFirstPerson(_firstPersonToggle.isOn);
        OnCameraValuesChanged?.Invoke();
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

    public void SetCameraFOV(float sliderValue)
    {
        PlayerPrefs.SetFloat(CAMERA_FOV, sliderValue);
        _cameraOptions.SetFOV(sliderValue);
        OnCameraValuesChanged?.Invoke();
    }

    public void SetCameraSideValue(float sliderValue)
    {
        PlayerPrefs.SetFloat(CAMERA_SIDE, sliderValue);
        _cameraOptions.SetCamearSide(sliderValue);
        OnCameraValuesChanged?.Invoke();
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
