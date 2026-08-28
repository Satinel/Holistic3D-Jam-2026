using UnityEngine;

public class CrushTrap : Trap
{
    [SerializeField] float _enemyCrushScale = 0.15f, _crushDuration = 1.75f;
    bool _isRecharging, _hasTriggered;
    float _timer;

    void OnTriggerEnter(Collider other)
    {
        if(_isRecharging) { return; }
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        Enemy detectedEnemy = null;

        if(other.TryGetComponent(out Enemy enemy))
        {
            detectedEnemy = enemy;
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            detectedEnemy = detector.ThisEnemy;
        }

        if(detectedEnemy == null || detectedEnemy.EnemyHealth.IsDead) { return; }

        if(!_hasTriggered)
        {
            _timer = 0;
            _hasTriggered = true;
            _animator.SetTrigger(TRIGGER_HASH);
            _audioSource.Play();
        }
    }

    void Update()
    {
        if(_isRecharging)
        {
            _timer += Time.deltaTime;

            if(_timer >= RechargeTime)
            {
                _hasTriggered = false;
                _isRecharging = false;
                _timer = 0;
            }
        }
    }

    public override void HitEnemy(Enemy enemy)
    {
        enemy.Crush(_enemyCrushScale, _crushDuration);

        if(Damage > 0)
        {
            enemy.EnemyHealth.LoseHealth(Damage);
        }
    }

    void SetRechargingAnimationEvent()
    {
        _isRecharging = true;
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _timer = RechargeTime;
    }
}
