using System.Collections.Generic;
using UnityEngine;

public class ZapTrap : Trap
{
    [SerializeField] MeshRenderer _modelRenderer;

    [SerializeField][ColorUsage(showAlpha:true, hdr:true)] Color _activeColor = Color.cyan, _unpoweredColor = Color.black;
    readonly HashSet<Enemy> _enemies = new();
    float _timer = Mathf.Infinity;
    bool _isActive = true, _isBright;

    static readonly int EMISSION_COLOR_ID = Shader.PropertyToID("_EmissionColor");

    void OnEnable()
    {
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed += Enemy_OnAnyEnemyDestroyed;
        Enemy.OnAnyEnemyColliderDisabled += Enemy_OnAnyEnemyColliderDisabled;
    }

    void OnDisable()
    {
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        Enemy.OnAnyEnemyDestroyed -= Enemy_OnAnyEnemyDestroyed;
        Enemy.OnAnyEnemyColliderDisabled -= Enemy_OnAnyEnemyColliderDisabled;
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

        if(_enemies.Count <= 0)
        {
            if(_isBright)
            {
                SetCoilMaterial(false);
            }
        }

        if(_timer < RechargeTime)
        {
            _timer += Time.deltaTime;
        }
        else if(_enemies.Count > 0)
        {
            bool hasTarget = false;
            HashSet<Enemy> _currentEnemies = new(_enemies);

            foreach(Enemy enemy in _currentEnemies)
            {
                if(!enemy || enemy.EnemyHealth.IsDead)
                {
                    _enemies.Remove(enemy);
                }
                else
                {
                    enemy.EnemyHealth.LoseHealth(Damage);
                    hasTarget = true;
                }
            }

            if(hasTarget)
            {
                _timer = 0;

                _audioSource.Play();

                if(!_isBright) { SetCoilMaterial(true); }
            }
        }
    }

    public void SetCoilMaterial(bool isBright)
    {
        _isBright = isBright;
        MaterialPropertyBlock mpb = new();
        _modelRenderer.GetPropertyBlock(mpb, 2);
        Color color = isBright ? _activeColor : _unpoweredColor;
        mpb.SetColor(EMISSION_COLOR_ID, color);
        _modelRenderer.SetPropertyBlock(mpb, 2);
    }

    void Enemy_OnAnyEnemyDestroyed(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }

    void Enemy_OnAnyEnemyColliderDisabled(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }

    void LevelManager_OnWaveCompleted(int index, int rewards)
    {
        if(_isBright) { SetCoilMaterial(false); }
        _isActive = false;
    }

    protected override void LevelManager_OnWaveStarted()
    {
        _isActive = true;
        _timer = RechargeTime;
    }
}
