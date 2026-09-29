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
    int _waveIndex = 0, _enemyIndex = 0;
    float _spawnTimer = 1f;
    bool _isSpawning = false;
    bool _pathBlocked;
    public bool IsSpawning => _isSpawning;

    void Awake()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        BarricadeTrap.OnAnyBarricadePlaced += CheckPath;
        BarricadeTrap.OnAnyBarricadeDestroyed += CheckPath;
    }

    void OnDestroy()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        BarricadeTrap.OnAnyBarricadePlaced -= CheckPath;
        BarricadeTrap.OnAnyBarricadeDestroyed -= CheckPath;
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

    void Enemy_OnAnyEnemyDestroyed(Enemy enemy)
    {
        _spawnedEnemies.Remove(enemy);
    }

    void CheckPath(BarricadeTrap newBarricade)
    {
        NavMeshPath path = new();
        _pathBlocked = !NavMesh.CalculatePath(transform.position, _core.transform.position, NavMesh.AllAreas, path);

        if(!_pathBlocked)
        {
            _blockingBarricades.Clear();
            _targetBarricade = null;
            foreach(Enemy enemy in _spawnedEnemies)
            {
                enemy.SetTargetBarricade(null);
            }
        }
        else
        {
            _blockingBarricades.Add(newBarricade);
            NavMeshPath pathToBlock = new();
            if(NavMesh.CalculatePath(transform.position, newBarricade.transform.position, NavMesh.AllAreas, pathToBlock))
            {
                _targetBarricade = newBarricade;
                newBarricade.SetShouldDestroy();

                foreach(Enemy enemy in _spawnedEnemies)
                {
                    enemy.SetTargetBarricade(newBarricade);
                }
            }
            else
            {
                foreach(BarricadeTrap barricadeTrap in _blockingBarricades)
                {
                    if(NavMesh.CalculatePath(transform.position, barricadeTrap.transform.position, NavMesh.AllAreas, pathToBlock))
                    {
                        _targetBarricade = barricadeTrap;
                        barricadeTrap.SetShouldDestroy();

                        foreach(Enemy enemy in _spawnedEnemies)
                        {
                            enemy.SetTargetBarricade(barricadeTrap);
                        }
                        break;
                    }
                }
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
