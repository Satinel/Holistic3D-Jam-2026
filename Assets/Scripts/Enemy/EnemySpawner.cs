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
    Coroutine _checkDelayCoroutine;
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
            if(_targetBarricade)
            {
                enemy.SetTargetBarricade(_targetBarricade);
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

    void CheckPath(BarricadeTrap newBarricade)
    {
        if(_applicationQuitting || _levelComplete) { return; }
Debug.Log($"{name} is checking path");
        NavMeshPath path = new();

        if(NavMesh.CalculatePath(transform.position, _core.transform.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
Debug.Log($"{name} has found a path");
            _targetBarricade = null;
            foreach(Enemy enemy in _spawnedEnemies)
            {
                enemy.SetTargetBarricade(null);
            }

            foreach(BarricadeTrap barricadeTrap in _blockingBarricades)
            {
                if(barricadeTrap)
                {
                    barricadeTrap.VoteToIgnore();
                }
            }
            _blockingBarricades.Clear();
        }
        else
        {
            if(newBarricade != null)
            {
                _blockingBarricades.Add(newBarricade);
            }

            if(_targetBarricade != null)
            {
                if(NavMesh.FindClosestEdge(_targetBarricade.transform.position, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    if(NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
Debug.Log("Maintaining path to targetBarricade");
                        return;
                    }
                }
            }

            if(newBarricade != null && NavMesh.FindClosestEdge(newBarricade.transform.position, out NavMeshHit newHit, NavMesh.AllAreas) 
                                    && NavMesh.CalculatePath(transform.position, newHit.position, NavMesh.AllAreas, path) 
                                    && path.status == NavMeshPathStatus.PathComplete)
            {
                _targetBarricade = newBarricade;
                newBarricade.VoteToDestroy();
Debug.Log("newBarricade set as targetBarricade");
                foreach(Enemy enemy in _spawnedEnemies)
                {
                    enemy.SetTargetBarricade(newBarricade);
                }
            }
            else
            {
Debug.Log("Searching for pathable barricade");
                foreach(BarricadeTrap barricadeTrap in _blockingBarricades)
                {
                    if(barricadeTrap == null) { continue; }

                    if(NavMesh.FindClosestEdge(barricadeTrap.transform.position, out NavMeshHit navHit, NavMesh.AllAreas)
                        && NavMesh.CalculatePath(transform.position, navHit.position, NavMesh.AllAreas, path) 
                        && path.status == NavMeshPathStatus.PathComplete)
                    {
Debug.Log("Pathable barricade found");
                        _targetBarricade = barricadeTrap;
                        barricadeTrap.VoteToDestroy();

                        foreach(Enemy enemy in _spawnedEnemies)
                        {
                            enemy.SetTargetBarricade(barricadeTrap);
                        }
                        break;
                    }
                }
Debug.Log("Search Complete");
            }
        }
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
