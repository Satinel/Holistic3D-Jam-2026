using System.Collections;
using UnityEngine;

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

    int _waveIndex = 0, _enemyIndex = 0;
    float _spawnTimer = 1f;
    bool _isSpawning = false;
    public bool IsSpawning => _isSpawning;

    void Awake()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
    }

    void OnDestroy()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
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
            Instantiate(_waves[_waveIndex].Enemies[_enemyIndex], _spawnPoints[Random.Range(0, _spawnPoints.Length)].position, transform.rotation, transform);

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
        if(!_isActive && index >= _activationIndex)
        {
            Activate();
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
