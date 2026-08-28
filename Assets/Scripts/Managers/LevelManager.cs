using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static event Action OnWaveStarted, OnLevelCompleted, OnLevelFailed;
    public static event Action<int> AnnounceWaves, OnWaveCompleted;

    [SerializeField] int _totalWaves = 1;
    [SerializeField] Canvas _winCanvas, _loseCanvas;
    [SerializeField] EnemySpawner[] _spawners;
    [SerializeField] int[] _waveRewards;

    bool _wavesActive, _levelWon, _levelLost, _isLoading = true;
    int _waveIndex;
    HashSet<Enemy> _activeEnemies = new();

    void Awake()
    {
        InputManager.OnUnleashPressed += InputManager_OnUnleashPressed;
        Enemy.OnAnyEnemySpawned += Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed += Core_OnCoreDestroyed;
    }

    void OnDestroy()
    {
        InputManager.OnUnleashPressed -= InputManager_OnUnleashPressed;
        Enemy.OnAnyEnemySpawned -= Enemy_OnAnyEnemySpawned;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        Core.OnCoreDestroyed -= Core_OnCoreDestroyed;
    }

    void Start()
    {
        AnnounceWaves?.Invoke(_totalWaves);
        _isLoading = false;
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
            spawner.StartSpawning(_waveIndex);
        }
        OnWaveStarted?.Invoke();
    }

    void LoadNextLevel()
    {
        if(_isLoading) { return; }

        _isLoading = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void ReloadLevel()
    {
        if(_isLoading) { return; }

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
            OnWaveCompleted?.Invoke(_waveRewards[_waveIndex]);
        }
    }

    void Core_OnCoreDestroyed()
    {
        OnLevelFailed?.Invoke();    // TODO : SFX/Music
        _loseCanvas.enabled = true;
        _levelLost = true;
    }
}
