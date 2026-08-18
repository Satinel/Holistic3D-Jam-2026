using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public event System.Action OnFinalWaveSpawned;

    [System.Serializable] class Wave
    {
        public Enemy[] Enemies;
    }

    [SerializeField] Wave[] _waves;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] float _minSpawnTime = 0.35f, _maxSpawnTime = 0.85f;

    int _waveIndex, _enemyIndex;
    float _spawnTimer = 1f;
    bool _isSpawning, _isFinished;

    void Update()
    {
        if(!_isSpawning || _isFinished) { return; }
        if(_waveIndex >= _waves.Length) { return; }

        if(_spawnTimer > 0)
        {
            _spawnTimer -= Time.deltaTime;
            return;
        }
        else
        {
            _spawnTimer = 0;
        }

        if(_waves[_waveIndex].Enemies.Length < _enemyIndex)
        {
            Enemy enemy = Instantiate(_waves[_waveIndex].Enemies[_enemyIndex], _spawnPoints[Random.Range(0, _spawnPoints.Length)].position, Quaternion.identity, transform);
            // TODO : Initialize enemy

            _enemyIndex++;

            if(_waves[_waveIndex].Enemies.Length >= _enemyIndex)
            {
                _isSpawning = false;
                _enemyIndex = 0;
                _waveIndex++;

                if(_waveIndex >= _waves.Length)
                {
                    _isFinished = true;
                    OnFinalWaveSpawned?.Invoke();
                }
            }
            else
            {
                _spawnTimer += Random.Range(_minSpawnTime, _maxSpawnTime);
            }
        }
    }

    public void BeginSpawning()
    {
        _enemyIndex = 0;
        _isSpawning = true;
    }
}
