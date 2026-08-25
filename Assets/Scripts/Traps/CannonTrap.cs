using UnityEngine;

public class CannonTrap : Trap
{
    [SerializeField] float _forceMultiplyer = 50f;
    [SerializeField] Cannonball _cannonballPrefab;
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
        Cannonball cannonball = Instantiate(_cannonballPrefab, _spawnPoint.position, _spawnPoint.rotation);
        cannonball.Initialize(Damage);
        cannonball.Rigidbody.AddForce(cannonball.transform.forward * _forceMultiplyer, ForceMode.VelocityChange);
    }

    void LevelManager_OnWaveCompleted(int _)
    {
        _canFire = false;
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _canFire = true;
    }
}
