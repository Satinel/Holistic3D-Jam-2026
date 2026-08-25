using UnityEngine;

public class MineTrap : Trap
{
    [SerializeField] float _forceMultiplyer = 50f;
    [SerializeField] GameObject _mine;
    [SerializeField] Payload _explosionRadius;
    [SerializeField] Collider _mainCollider;

    bool _isRecharging;
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

        Explode();
    }

    void Update()
    {
        if(_isRecharging)
        {
            _timer += Time.deltaTime;
            
            if(_timer >= RechargeTime)
            {
                _isRecharging = false;
                _timer = 0;
                _mine.SetActive(true);
                _mainCollider.enabled = true;
            }
        }
    }

    void Explode()
    {
        _mainCollider.enabled = false;
        _timer = 0;
        _isRecharging = true;

        _explosionRadius.gameObject.SetActive(true);
        // TODO : Explosion particle effects (it can auto-play as a component of _explosionRadius)

        _mine.SetActive(false);
    }

    public override void HitEnemy(Enemy enemy)
    {
        enemy.AccurateRagdoll((enemy.transform.position - transform.position + Vector3.up).normalized * _forceMultiplyer, ForceMode, RagdollDuration);

        if(Damage > 0)
        {
            enemy.Health.LoseHealth(Damage);
        }
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _timer = RechargeTime;
    }
}
