using System;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static event Action<float> OnTimeScaleChanged;

    [SerializeField] float _increasedTimeScale = 2f;
    readonly float _defaultTimeScale = 1f;
    float _currentTimeScale = 1f;
    bool _levelStarted, _levelComplete, _gamePaused, _usingNormalSpeed = true;

    void OnEnable()
    {
        InputManager.OnTimeScalePressed += InputManager_OnTimeScalePressed;
        OptionsMenu.OnOptionsCanvasToggled += OptionsMenu_OnOptionsCanvasToggled;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelStarted += LevelManager_OnLevelStarted;
    }

    void OnDisable()
    {
        InputManager.OnTimeScalePressed -= InputManager_OnTimeScalePressed;
        OptionsMenu.OnOptionsCanvasToggled -= OptionsMenu_OnOptionsCanvasToggled;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelStarted -= LevelManager_OnLevelStarted;
    }

    void InputManager_OnTimeScalePressed()
    {
        if(!_levelStarted) { return; }
        if(_gamePaused) { return; }
        if(_levelComplete) { return; }

        Time.timeScale = _currentTimeScale = _usingNormalSpeed ? _increasedTimeScale : _defaultTimeScale;   // Unnecessarily fancy for the sake of learning
        _usingNormalSpeed = !_usingNormalSpeed;
        OnTimeScaleChanged?.Invoke(_currentTimeScale);
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool isEnabled)
    {
        _gamePaused = isEnabled;

        if(_gamePaused)
        {
            Time.timeScale = 0;
        }
        else if(_levelStarted && !_levelComplete)
        {
            Time.timeScale = _currentTimeScale;
        }

        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    void LevelManager_OnLevelCompleted()
    {
        _levelComplete = true;
        Time.timeScale = 0;
        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    void LevelManager_OnLevelStarted()
    {
        Time.timeScale = _currentTimeScale;
        _levelStarted = true;
    }
}
