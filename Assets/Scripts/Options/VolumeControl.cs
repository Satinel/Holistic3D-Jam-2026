using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public static event Action<bool> OnAudioCanvasToggled;
    public static event Action OnRestartRequested;

    public AudioMixer AudioMixer;
    [SerializeField] GameObject _mainMenuButton, _cancelButton, _cancelRestartButton, _restartPrompt, _quitPrompt;
    [SerializeField] Canvas _audioCanvas;
    [SerializeField] Slider _mainVolumeSlider;
    [SerializeField] Slider _musicVolumeSlider;
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] Toggle _mainMuteToggle, _musicMuteToggle, _sfxMuteToggle;
    // [SerializeField] OptionsMenu _optionsMenu;

    bool _sceneChanging;

    void Awake()
    {
        InputManager.OnOptionsPressed += ToggleAudioCanvas;
        LevelManager.OnSceneChangeStarted += LevelManager_OnScenChangeStarted;
    }

    void OnDestroy()
    {
        InputManager.OnOptionsPressed -= ToggleAudioCanvas;
        LevelManager.OnSceneChangeStarted -= LevelManager_OnScenChangeStarted;
    }

    void Start()
    {
        _mainVolumeSlider.value = PlayerPrefs.GetFloat("MainVolume", 1);
        _musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1);

        _mainMuteToggle.isOn = PlayerPrefs.GetInt("MainMuted", 0) == 1;
        ToggleMuteMainVolume();

        _musicMuteToggle.isOn = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        ToggleMuteMusicVolume();

        _sfxMuteToggle.isOn = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        ToggleMuteSFXVolume();
    }

    public void SetMainVolumeLevel(float sliderValue)
    {
        PlayerPrefs.SetFloat("MainVolume", sliderValue);

        if(_mainMuteToggle.isOn) { return; }

        AudioMixer.SetFloat("MainVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void ToggleMuteMainVolume()
    {
        if(_mainMuteToggle.isOn)
        {
            PlayerPrefs.SetInt("MainMuted", 1);
            AudioMixer.SetFloat("MainVolume", Mathf.Log10(0.0001f) * 20);  // 0.0001f works but 0 doesn't because Log10 stuff I don't understand
        }
        else
        {
            PlayerPrefs.SetInt("MainMuted", 0);
            SetMainVolumeLevel(_mainVolumeSlider.value);
        }
    }

    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);

        if(_musicMuteToggle.isOn) { return; }

        AudioMixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void ToggleMuteMusicVolume()
    {
        if(_musicMuteToggle.isOn)
        {
            PlayerPrefs.SetInt("MusicMuted", 1);
            AudioMixer.SetFloat("MusicVolume", Mathf.Log10(0.0001f) * 20);  // 0.0001f works but 0 doesn't because Log10 stuff I don't understand
        }
        else
        {
            PlayerPrefs.SetInt("MusicMuted", 0);
            SetMusicVolume(_musicVolumeSlider.value);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);

        if(_sfxMuteToggle.isOn) { return; }

        AudioMixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void ToggleMuteSFXVolume()
    {
        if(_sfxMuteToggle.isOn)
        {
            PlayerPrefs.SetInt("SFXMuted", 1);
            AudioMixer.SetFloat("SFXVolume", Mathf.Log10(0.0001f) * 20);  // 0.0001f works but 0 doesn't because Log10 stuff I don't understand
        }
        else
        {
            PlayerPrefs.SetInt("SFXMuted", 0);
            SetSFXVolume(_sfxVolumeSlider.value);
        }
    }

    void ToggleAudioCanvas()
    {
        if(_sceneChanging) { return; }

        _audioCanvas.enabled = !_audioCanvas.enabled;
        if(_audioCanvas.enabled)
        {
            EnableAudioCanvas();
        }
        else
        {
            DisableAudioCanvas();
        }
    }

    public void DisableAudioCanvas()
    {
        _audioCanvas.enabled = false;
        EventSystem.current.SetSelectedGameObject(null);
        OnAudioCanvasToggled?.Invoke(false);

        // _optionsMenu.EnableOptionsCanvas();
    }

    void EnableAudioCanvas()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_mainMenuButton);
        OnAudioCanvasToggled?.Invoke(true);
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
        EventSystem.current.SetSelectedGameObject(_mainMenuButton);
    }

    public void RestartLevel()
    {
        if(_sceneChanging) { return; }

        DisableAudioCanvas();
        _sceneChanging = true;
        OnRestartRequested?.Invoke();
    }

    public void PromptQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _quitPrompt.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_cancelButton);
    }

    public void CancelQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _quitPrompt.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_mainMenuButton);
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
