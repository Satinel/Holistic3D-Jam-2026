using System.Collections.Generic;
using UnityEngine;

public class ZapTrap : Trap
{
    readonly HashSet<Enemy> _enemies = new();
    float _timer;
    bool _isActive = true;

    void OnEnable()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
    }

    void OnDisable()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            if(!enemy.EnemyHealth.IsDead)
            {
                _enemies.Add(enemy);
            }
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            if(!detector.ThisEnemy.EnemyHealth.IsDead)
            {
                _enemies.Add(enemy);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag(ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            _enemies.Remove(enemy);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            _enemies.Remove(detector.ThisEnemy);
        }
    }

    void Update()
    {
        if(!_isActive) { return; }
        if(_timer < RechargeTime)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            if(_enemies.Count <= 0) { return; }

            _timer -= RechargeTime;
            foreach(Enemy enemy in _enemies)
            {
                if(!enemy || enemy.EnemyHealth.IsDead) { continue; }

                enemy.EnemyHealth.LoseHealth(Damage);
            }
            _audioSource.Play();
        }
    }

    void Enemy_OnAnyEnemyDestroyed(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }

    void LevelManager_OnWaveCompleted(int index, int rewards)
    {
        _isActive = false;
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _isActive = true;
        _timer = RechargeTime;
    }
}
