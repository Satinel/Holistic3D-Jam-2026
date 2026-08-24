using UnityEngine;

public class PushTrap : Trap
{
    [SerializeField] float _forceMultiplyer;
    [SerializeField] Vector3 _forceDirection = Vector3.up;

    bool _hasTriggered, _isRecharging;
    float _timer;

    void Start()
    {
        _forceDirection = transform.up;
    }

    void OnTriggerEnter(Collider other)
    {
        if(_isRecharging) { return; }

        if(!_hasTriggered && other.CompareTag(ENEMY_TAG))
        {
            _hasTriggered = true;
            _animator.SetTrigger(TRIGGER_HASH);
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
