using UnityEngine;

public class SpringTrap : Trap
{
    [SerializeField] float _forceMultiplyer;
    [SerializeField] Vector3 _forceDirection = Vector3.up;

    bool _hasTriggered, _isRecharging;
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

        if(detectedEnemy == null || detectedEnemy.Health.IsDead) { return; }

        if(!_hasTriggered)
        {
            _hasTriggered = true;
            _animator.SetTrigger(TRIGGER_HASH);
        }

        HitEnemy(detectedEnemy);
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
        enemy.AccurateRagdoll(_forceDirection * _forceMultiplyer, ForceMode, RagdollDuration);
        if(Damage > 0)
        {
            enemy.Health.LoseHealth(Damage);
        }
    }

    void SetRechargingAnimationEvent()
    {
        _isRecharging = true;
    }
}
