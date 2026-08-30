using System;
using System.Collections.Generic;
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

    bool _wavesActive, _levelWon, _levelLost, _isLoading = true, _hasStarted;
    int _waveIndex;
    HashSet<Enemy> _activeEnemies = new();
    IDisposable _eventListener;

    void Awake()
    {
        InputManager.OnUnleashPressed += InputManager_OnUnleashPressed;
        VolumeControl.OnRestartRequested += VolumeControl_OnRestartRequested;
        Enemy.OnAnyEnemySpawned += Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed += Core_OnCoreDestroyed;
    }

    void OnDestroy()
    {
        InputManager.OnUnleashPressed -= InputManager_OnUnleashPressed;
        VolumeControl.OnRestartRequested -= VolumeControl_OnRestartRequested;
        Enemy.OnAnyEnemySpawned -= Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed -= Core_OnCoreDestroyed;
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
        _eventListener.Dispose();
        _eventListener = null;
        if(_hasStarted) { return; }

        _hasStarted = true;
        _splashCanvas.enabled = false;
        OnLevelStarted?.Invoke();
    }

    void InputManager_OnUnleashPressed()
    {
        if(_isLoading) { return; }

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

    void VolumeControl_OnRestartRequested()
    {
        ReloadLevel();
    }

    void LoadNextLevel()
    {
        if(_isLoading) { return; }

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
            OnLevelCompleted?.Invoke(); // TODO : SFX/Music
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
        OnLevelFailed?.Invoke();    // TODO : SFX/Music
        _loseCanvas.enabled = true;
        _levelLost = true;
    }
}
