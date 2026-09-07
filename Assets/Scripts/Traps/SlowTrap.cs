using UnityEngine;

public class SlowTrap : Trap
{
    [SerializeField] GameObject _slowField;

    float _timer = 0;

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        Enemy detectedEnemy = null;

        if(other.TryGetComponent(out Enemy enemy))
        {
            detectedEnemy = enemy;
        }

        if(detectedEnemy == null || detectedEnemy.EnemyHealth.IsDead) { return; }

        _timer = 0;
        _slowField.SetActive(true);

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.Play();

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
}
