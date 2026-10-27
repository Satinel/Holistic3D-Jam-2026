using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static event Action OnWaveStarted, OnLevelCompleted, OnLevelFailed, OnLevelStarted, OnSceneChangeStarted;
    public static event Action<int> AnnounceWaves;
    public static event Action<int, int> OnWaveCompleted;

    [SerializeField] int _totalWaves = 1;
    [SerializeField] Canvas _splashCanvas, _winCanvas, _loseCanvas, _loadCanvas;
    [SerializeField] EnemySpawner[] _spawners;
    [SerializeField] int[] _waveRewards;
    [SerializeField] TextMeshProUGUI _nextLevelText, _restartText;
    [SerializeField] NewGamePlusSO _newGamePlusSO;

    bool _wavesActive, _levelWon, _levelLost, _isLoading = true, _hasStarted, _isInOptions;
    int _waveIndex;
    HashSet<Enemy> _activeEnemies = new();
    IDisposable _eventListener;
    InputAction _unleashAction;

    void Awake()
    {
        InputManager.ReportUnleashAction += InputManager_ReportUnleashAction;
        InputManager.OnUnleashPressed += InputManager_OnUnleashPressed;
        OptionsMenu.OnOptionsCanvasToggled += OptionsMenu_OnOptionsCanvasToggled;
        OptionsMenu.OnRestartRequested += OptionsMenu_OnRestartRequested;
        Enemy.OnAnyEnemySpawned += Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed += Core_OnCoreDestroyed;
    }

    void OnDestroy()
    {
        InputManager.ReportUnleashAction -= InputManager_ReportUnleashAction;
        InputManager.OnUnleashPressed -= InputManager_OnUnleashPressed;
        OptionsMenu.OnOptionsCanvasToggled -= OptionsMenu_OnOptionsCanvasToggled;
        OptionsMenu.OnRestartRequested -= OptionsMenu_OnRestartRequested;
        Enemy.OnAnyEnemySpawned -= Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed -= Core_OnCoreDestroyed;

        _eventListener?.Dispose();
        _eventListener = null;
    }

    void Start()
    {
        _eventListener = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
        AnnounceWaves?.Invoke(_totalWaves);
        _isLoading = false;
    }

    void OnButtonPressed(InputControl button)
    {
        // I really don't know why the 'new' input system needs such convoluted means to check for any input, when the 'bad' system it replaced handled it with ease
        _eventListener?.Dispose();
        _eventListener = null;
        if(_hasStarted) { return; }

        _hasStarted = true;
        _splashCanvas.enabled = false;
        OnLevelStarted?.Invoke();
    }

    void InputManager_ReportUnleashAction(InputAction unleashAction)
    {
        _unleashAction = unleashAction;
        SetUnleashTexts();
    }

    void SetUnleashTexts()
    {
        int index = 0;  // TODO Set up determining which control scheme is used and set index appropriately (0 for keyboard/mouse and 1 for gamepad)
        string unleashBind = _unleashAction.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        _nextLevelText.text = $"Press {unleashBind} For Next Level";
        _restartText.text = $"Press {unleashBind} To Restart";
    }

    void InputManager_OnUnleashPressed()
    {
        if(!_hasStarted) { return; }
        if(_isLoading) { return; }
        if(_isInOptions) { return; }

        if(_levelWon)
        {
            LoadNextLevel();
            return;
        }

        if(_levelLost)
        {
            ReloadLevel();
            return;
        }

        if(_wavesActive) { return; }

        _wavesActive = true;
        foreach(EnemySpawner spawner in _spawners)
        {
            if(spawner.gameObject.activeSelf)
            {
                spawner.StartSpawning(_waveIndex);
            }
        }
        OnWaveStarted?.Invoke();
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool isInOptions)
    {
        _isInOptions = isInOptions;
    }

    void OptionsMenu_OnRestartRequested()
    {
        ReloadLevel();
    }

    void LoadNextLevel()
    {
        if(_isLoading) { return; }

        if(_newGamePlusSO)
        {
            _newGamePlusSO.SetNewGamePlus();
        }

        _loadCanvas.enabled = true;
        OnSceneChangeStarted?.Invoke();
        _isLoading = true;
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;

        SceneManager.LoadScene(nextSceneIndex);
    }

    void ReloadLevel()
    {
        if(_isLoading) { return; }

        OnSceneChangeStarted?.Invoke();
        _isLoading = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Enemy_OnAnyEnemySpawned(Enemy enemy)
    {
        _activeEnemies.Add(enemy);
    }

    void Enemy_OnAnyEnemyDestroyed(Enemy enemy)
    {
        _activeEnemies.Remove(enemy);

        if(_activeEnemies.Count <= 0)
        {
            CheckWaveComplete();
        }
    }

    void CheckWaveComplete()
    {
        foreach(EnemySpawner spawner in _spawners)
        {
            if(spawner.IsSpawning)
            {
                return;
            }
        }

        _wavesActive = false;
        _waveIndex++;

        if(_waveIndex >= _totalWaves)
        {
            OnLevelCompleted?.Invoke();
            SetUnleashTexts();
            _winCanvas.enabled = true;
            _levelWon = true;
        }
        else
        {
            OnWaveCompleted?.Invoke(_waveIndex, _waveRewards[_waveIndex]);
        }
    }

    void Core_OnCoreDestroyed()
    {
        OnLevelFailed?.Invoke();
        SetUnleashTexts();
        _loseCanvas.enabled = true;
        _levelLost = true;
    }
}
