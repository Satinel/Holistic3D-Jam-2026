using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable] class Wave
    {
        public Enemy[] Enemies;
    }

    [SerializeField] Wave[] _waves;
    [SerializeField] Transform[] _spawnPoints;
    [SerializeField] float _minSpawnTime = 0.35f, _maxSpawnTime = 0.85f;

    int _waveIndex = 0, _enemyIndex = 0;
    float _spawnTimer = 1f;
    bool _isSpawning = false;
    public bool IsSpawning => _isSpawning;
    public int TotalWaves => _waves.Length;

    void Update()
    {
        if(!_isSpawning) { return; }
        if(_waveIndex >= _waves.Length) { return; }
        if(_waves[_waveIndex].Enemies.Length <= 0) { return; }

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

    void BeginSpawning()
    {
        _spawnTimer = 1;
        _enemyIndex = 0;
        _isSpawning = true;
    }

    public void StartSpawning(int index)
    {
        if(_waves.Length <= 0) { return; }

        _waveIndex = index % _waves.Length;

        if(gameObject.activeSelf && !_isSpawning)
        {
            BeginSpawning();
        }
    }
}
