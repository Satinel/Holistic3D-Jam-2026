using UnityEngine;

public class BoulderTrap : Trap
{
    [SerializeField] float _forceMultiplyer = 50f, _enemyCrushScale = 0.15f, _crushDuration = 1.25f;
    [SerializeField] Payload _boulderPrefab;
    [SerializeField] Transform _spawnPoint;

    bool _canFire = true;
    float _timer;

    void Start()
    {
        _timer = RechargeTime;
    }

    void OnEnable()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
    }

    void OnDisable()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
    }

    void Update()
    {
        if(_canFire)
        {
            if(_timer < RechargeTime)
            {
                _timer += Time.deltaTime;
            }

            if(_timer >= RechargeTime)
            {
                Fire();
                _timer -= RechargeTime;
            }
        }
    }

    void Fire()
    {
        _audioSource.Play();
        Payload boulder = Instantiate(_boulderPrefab, _spawnPoint.position, _spawnPoint.rotation);
        boulder.Initialize(this);
        boulder.GetComponent<Rigidbody>().AddForce(boulder.transform.forward * _forceMultiplyer, ForceMode.VelocityChange);
    }

    public override void HitEnemy(Enemy enemy)
    {
        enemy.Crush(_enemyCrushScale, _crushDuration);

        if(Damage > 0)
        {
            enemy.EnemyHealth.LoseHealth(Damage);
        }
    }

    void LevelManager_OnWaveCompleted(int index, int rewards)
    {
        _canFire = false;
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _canFire = true;
    }
}
