using UnityEngine;

public class SlowTrap : Trap, ISapable
{
    [SerializeField] GameObject _slowField;

    float _timer = 0, _sapTimer = 0;
    bool _isDisabled;

    void OnTriggerEnter(Collider other)
    {
        if(_isDisabled) { return; }
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        Enemy detectedEnemy = null;

        if(other.TryGetComponent(out Enemy enemy))
        {
            detectedEnemy = enemy;
        }

        if(detectedEnemy == null || detectedEnemy.EnemyHealth.IsDead) { return; }

        _timer = 0;

        if(!_slowField.activeSelf)
        {
            _slowField.SetActive(true);
            _audioSource.Play();
        }

        HitEnemy(detectedEnemy);
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        Enemy detectedEnemy = null;

        if(other.TryGetComponent(out Enemy enemy))
        {
            detectedEnemy = enemy;
        }

        if(detectedEnemy == null || detectedEnemy.EnemyHealth.IsDead) { return; }

        ReleaseEnemy(detectedEnemy);
    }

    void Update()
    {
        if(_isDisabled)
        {
            _sapTimer += Time.deltaTime;

            if(_sapTimer >= 0)
            {
                _sapTimer = 0;
                _isDisabled = false;
            }
            else
            {
                return;
            }
        }

        if(_timer < RechargeTime)
        {
            _timer += Time.deltaTime;

            if(_timer >= RechargeTime)
            {
                _slowField.SetActive(false);
            }
        }
    }

    public override void HitEnemy(Enemy enemy)
    {
        enemy.Slow();
    }

    void ReleaseEnemy(Enemy enemy)
    {
        enemy.RecoverFromSlow();
    }

    public void Sap(float duration)
    {
        _slowField.SetActive(false);
        _isDisabled = true;
        _sapTimer -= duration;
    }
}
