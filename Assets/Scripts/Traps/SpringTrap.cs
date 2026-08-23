using UnityEngine;

public class SpringTrap : Trap
{
    [SerializeField] float _forceMultiplyer;
    [SerializeField] Vector3 _forceDirection = Vector3.up;

    bool _hasTriggered;
    float _timer;

    void OnTriggerEnter(Collider other)
    {
        if(!_hasTriggered && other.CompareTag(ENEMY_TAG))
        {
            _animator.SetTrigger(TRIGGER_HASH);
        }

        if(other.TryGetComponent(out Enemy enemy))
        {
            HitEnemy(enemy);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))   // TODO? : Replace this with a new class attached to Ragdoll Rigidbody (if this doesn't work)
        {
Debug.Log("It worked");
            HitEnemy(detector.ThisEnemy);
        }
    }

    void Update()
    {
        if(_hasTriggered)
        {
            _timer += Time.deltaTime;

            if(_timer >= RechargeTime)
            {
                _hasTriggered = false;
                _timer = 0;
            }
        }
    }

    void HitEnemy(Enemy enemy)
    {
        enemy.AccurateRagdoll(_forceDirection * _forceMultiplyer, ForceMode, RagdollDuration);
        if(Damage > 0)
        {
            enemy.Health.LoseHealth(Damage);
        }
    }
}
