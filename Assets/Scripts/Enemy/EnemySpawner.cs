using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public static event System.Action OnAnySpawnerActivated;

    [System.Serializable] class Wave
    {
        public Enemy[] Enemies;
    }

    [SerializeField] int _activationIndex = 0;
    [SerializeField] Wave[] _waves;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] float _minSpawnTime = 0.35f, _maxSpawnTime = 0.85f;
    [SerializeField] bool _isActive;
    [SerializeField] GameObject _visualsParent;
    [SerializeField] Path _path;
    [SerializeField] Core _core;

    readonly HashSet<Enemy> _spawnedEnemies = new();
    readonly HashSet<BarricadeTrap> _blockingBarricades = new();

    BarricadeTrap _targetBarricade;
    Transform _barricadeAttackPoint;
    Coroutine _checkDelayCoroutine, _enemyTargetingCoroutine;
    int _waveIndex = 0, _enemyIndex = 0;
    float _spawnTimer = 1f;
    bool _isSpawning = false;
    bool _levelComplete, _applicationQuitting;
    public bool IsSpawning => _isSpawning;

    void Awake()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        BarricadeTrap.OnAnyBarricadePlaced += BarricadeTrap_OnAnyBarricadePlaced;
        BarricadeTrap.OnAnyBarricadeDestroyed += BarricadeTrap_OnAnyBarricadeDestroyed;
    }

    void OnDestroy()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        BarricadeTrap.OnAnyBarricadePlaced -= BarricadeTrap_OnAnyBarricadePlaced;
        BarricadeTrap.OnAnyBarricadeDestroyed -= BarricadeTrap_OnAnyBarricadeDestroyed;
    }

    void OnApplicationQuit()
    {
        _applicationQuitting = true;
    }

    void Start()
    {
        if(_activationIndex == 0)
        {
            _isActive = true;
            _visualsParent.SetActive(true);
        }
        else if(!_isActive)
        {
            _visualsParent.SetActive(false);

            if(_path)
            {
                _path.DeactivatePassage();
            }
        }
    }

    void Update()
    {
        if(!_isActive) { return; }
        if(!_isSpawning) { return; }

        if(_spawnTimer > 0)
        {
            _spawnTimer -= Time.deltaTime;
            return;
        }
        else
        {
            _spawnTimer = 0;
        }

        if(_enemyIndex < _waves[_waveIndex].Enemies.Length)
        {
            Enemy enemy = Instantiate(_waves[_waveIndex].Enemies[_enemyIndex], _spawnPoints[Random.Range(0, _spawnPoints.Length)].position, transform.rotation, transform);
            _spawnedEnemies.Add(enemy);
            enemy.SetCore(_core);
            if(_targetBarricade && _barricadeAttackPoint)
            {
                enemy.SetTargetBarricade(_targetBarricade, _barricadeAttackPoint);
            }

            _enemyIndex++;

            if(_enemyIndex >= _waves[_waveIndex].Enemies.Length)
            {
                _isSpawning = false;
                _enemyIndex = 0;
            }
            else
            {
                _spawnTimer += Random.Range(_minSpawnTime, _maxSpawnTime);
            }
        }
    }

    public void StartSpawning(int index)
    {
        if(!_isActive) { return; }
        if(_waves.Length <= 0) { return; }

        _waveIndex = index % _waves.Length;

        if(gameObject.activeSelf && !_isSpawning)
        {
            BeginSpawning();
        }
    }

    void BeginSpawning()
    {
        if(_waveIndex >= _waves.Length) { return; }
        if(_waves[_waveIndex].Enemies.Length <= 0) { return; }

        _spawnTimer = 1;
        _enemyIndex = 0;
        _isSpawning = true;
    }

    void LevelManager_OnWaveCompleted(int index, int rewards)
    {
        _spawnedEnemies.Clear();

        if(!_isActive && index >= _activationIndex)
        {
            Activate();
        }
    }

    void LevelManager_OnLevelCompleted()
    {
        _levelComplete = true;
    }

    void Enemy_OnAnyEnemyDestroyed(Enemy enemy)
    {
        _spawnedEnemies.Remove(enemy);
    }

    void BarricadeTrap_OnAnyBarricadePlaced(BarricadeTrap newBarricade)
    {
        if(_checkDelayCoroutine != null)
        {
            StopCoroutine(_checkDelayCoroutine);
        }

        _checkDelayCoroutine = StartCoroutine(BarricadePlacementDelayRoutine(newBarricade));
    }

    void BarricadeTrap_OnAnyBarricadeDestroyed(BarricadeTrap newBarricade)
    {
        if(_targetBarricade == newBarricade)
        {
            _targetBarricade = null;
            _barricadeAttackPoint = null;
        }

        if(_checkDelayCoroutine != null)
        {
            StopCoroutine(_checkDelayCoroutine);
        }

        _checkDelayCoroutine = StartCoroutine(BarricadePlacementDelayRoutine(newBarricade));
    }

    IEnumerator BarricadePlacementDelayRoutine(BarricadeTrap newBarricade)
    {
        yield return null;
        yield return null;

        _checkDelayCoroutine = null;
        CheckPath(newBarricade);
    }

    IEnumerator SetEnemiesTarget(HashSet<Enemy> enemies, BarricadeTrap barricade, Transform attackPoint)
    {
        HashSet<Enemy> activeEnemies = new(enemies);

        foreach(Enemy enemy in activeEnemies)
        {
            if(enemy)
            {
                enemy.SetTargetBarricade(barricade, attackPoint);
                yield return null;
            }
        }

        _enemyTargetingCoroutine = null;
    }

// [SerializeField] Transform _indicator;
    void CheckPath(BarricadeTrap newBarricade)
    {
        if(_applicationQuitting || _levelComplete) { return; }
// Debug.Log($"{name} is checking path");
        NavMeshPath path = new();

        if(NavMesh.CalculatePath(transform.position, _core.transform.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            _targetBarricade = null;
            _barricadeAttackPoint = null;

            foreach(BarricadeTrap barricadeTrap in _blockingBarricades)
            {
                if(barricadeTrap)
                {
                    barricadeTrap.VoteToIgnore();
                }
            }
            _blockingBarricades.Clear();

            if(_enemyTargetingCoroutine != null)
            {
                StopCoroutine(_enemyTargetingCoroutine);
            }
            _enemyTargetingCoroutine = StartCoroutine(SetEnemiesTarget(_spawnedEnemies, null, null));
// _indicator.transform.position = transform.position;
// Debug.Log($"{name} has found a path");
            return;
        }

        if(newBarricade != null)
        {
            _blockingBarricades.Add(newBarricade);
        }

        if(_targetBarricade != null)
        {
            for(int i = 0; i < _targetBarricade.AttackPoints.Length; i++)
            {
                if(NavMesh.CalculatePath(transform.position, _targetBarricade.AttackPoints[i].position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    _barricadeAttackPoint = _targetBarricade.AttackPoints[i];
// _indicator.transform.position = _targetBarricade.AttackPoints[i].position;
// Debug.Log("Maintaining path to targetBarricade");
                    return;
                }
            }
        }

        if(newBarricade != null)
        {
            for(int i = 0; i < newBarricade.AttackPoints.Length; i++)
            {
                if(NavMesh.CalculatePath(transform.position, newBarricade.AttackPoints[i].position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    _barricadeAttackPoint = newBarricade.AttackPoints[i];
                    _targetBarricade = newBarricade;
                    newBarricade.VoteToDestroy();

                    if(_enemyTargetingCoroutine != null)
                    {
                        StopCoroutine(_enemyTargetingCoroutine);
                    }
                    _enemyTargetingCoroutine = StartCoroutine(SetEnemiesTarget(_spawnedEnemies, _targetBarricade, _barricadeAttackPoint));
// _indicator.transform.position = newBarricade.AttackPoints[i].position;
// Debug.Log("newBarricade set as targetBarricade");
                    return;
                }
            }
        }
        else
        {
// Debug.Log("Searching for pathable barricade");
            foreach(BarricadeTrap barricadeTrap in _blockingBarricades)
            {
                if(barricadeTrap == null) { continue; }

                for(int i = 0; i < barricadeTrap.AttackPoints.Length; i++)
                {
                    if(NavMesh.CalculatePath(transform.position, barricadeTrap.AttackPoints[i].position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        _barricadeAttackPoint = barricadeTrap.AttackPoints[i];
                        _targetBarricade = barricadeTrap;
                        barricadeTrap.VoteToDestroy();

                        if(_enemyTargetingCoroutine != null)
                        {
                            StopCoroutine(_enemyTargetingCoroutine);
                        }
                        _enemyTargetingCoroutine = StartCoroutine(SetEnemiesTarget(_spawnedEnemies, _targetBarricade, _barricadeAttackPoint));
// _indicator.transform.position = barricadeTrap.AttackPoints[i].position;
// Debug.Log("Pathable barricade found");
                        return;
                    }
                }
            }
        }
// Debug.Log("No possible paths found (that's bad but in theory also impossible)");
    }

    void Activate()
    {
        if(_isActive) { return; }

        if(_path)
        {
            _path.ActivatePath();
        }
        _isActive = true;
        _visualsParent.SetActive(true);
        OnAnySpawnerActivated?.Invoke();
    }
}
