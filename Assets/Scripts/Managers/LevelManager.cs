using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static event Action OnWaveStarted, OnLevelCompleted, OnLevelFailed;
    public static event Action<int> OnWaveCompleted;

    [SerializeField] EnemySpawner[] _spawners;
    [SerializeField] int[] _waveRewards;

    bool _wavesActive;
    int _waveIndex, _totalWaves;
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
        foreach(EnemySpawner spawner in _spawners)
        {
            _totalWaves = spawner.TotalWaves > _totalWaves ? spawner.TotalWaves : _totalWaves;
        }
    }

    void InputManager_OnUnleashPressed()
    {
        if(_wavesActive) { return; }

        _wavesActive = true;
        foreach(EnemySpawner spawner in _spawners)
        {
            spawner.StartSpawning(_waveIndex);
        }
        OnWaveStarted?.Invoke();
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

        if(_waveIndex > _totalWaves)
        {
            OnLevelCompleted?.Invoke(); // TODO : Handle success state
        }
        else
        {
            OnWaveCompleted?.Invoke(_waveRewards[_waveIndex]);
        }
    }

    void Core_OnCoreDestroyed()
    {
        OnLevelFailed?.Invoke();    // TODO : Handle failure state
    }
}
