using System;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static event Action<float> OnTimeScaleChanged;

    [SerializeField] float _increasedTimeScale = 2f;
    readonly float _defaultTimeScale = 1f;
    float _currentTimeScale = 1f;
    bool _levelStarted, _levelOver, _optionsEnabled, _inventoryEnabled, _usingNormalSpeed = true;

    void OnEnable()
    {
        InputManager.OnTimeScalePressed += InputManager_OnTimeScalePressed;
        OptionsMenu.OnOptionsCanvasToggled += OptionsMenu_OnOptionsCanvasToggled;
        Inventory.OnInventoryToggled += Inventory_OnInventoryToggled;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.OnLevelStarted += LevelManager_OnLevelStarted;
    }

    void OnDisable()
    {
        InputManager.OnTimeScalePressed -= InputManager_OnTimeScalePressed;
        OptionsMenu.OnOptionsCanvasToggled -= OptionsMenu_OnOptionsCanvasToggled;
        Inventory.OnInventoryToggled -= Inventory_OnInventoryToggled;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed -= LevelManager_OnLevelFailed;
        LevelManager.OnLevelStarted -= LevelManager_OnLevelStarted;
    }

    void InputManager_OnTimeScalePressed()
    {
        if(!_levelStarted) { return; }
        if(_optionsEnabled || _inventoryEnabled) { return; }
        if(_levelOver) { return; }

        Time.timeScale = _currentTimeScale = _usingNormalSpeed ? _increasedTimeScale : _defaultTimeScale;   // Unnecessarily fancy for the sake of learning
        _usingNormalSpeed = !_usingNormalSpeed;
        OnTimeScaleChanged?.Invoke(_currentTimeScale);
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool optionsEnabled)
    {
        if(_levelOver) { return; }

        _optionsEnabled = optionsEnabled;

        if(_optionsEnabled || _inventoryEnabled)
        {
            Time.timeScale = 0;
        }
        else if(_levelStarted)
        {
            Time.timeScale = _currentTimeScale;
        }

        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    void Inventory_OnInventoryToggled(bool inventoryEnabled)
    {
        if(_levelOver) { return; }

        _inventoryEnabled = inventoryEnabled;

        if(_optionsEnabled || _inventoryEnabled)
        {
            Time.timeScale = 0;
        }
        else if(_levelStarted)
        {
            Time.timeScale = _currentTimeScale;
        }
    }

    void LevelManager_OnLevelCompleted()
    {
        _levelOver = true;
        Time.timeScale = 0;
        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    void LevelManager_OnLevelFailed()
    {
        _levelOver = true;
        Time.timeScale = 0;
        OnTimeScaleChanged?.Invoke(Time.timeScale);
    }

    void LevelManager_OnLevelStarted()
    {
        Time.timeScale = _currentTimeScale;
        _levelStarted = true;
    }
}
