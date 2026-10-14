using System.Collections.Generic;
using UnityEngine;

public class ZapTrap : Trap
{
    [SerializeField] MeshRenderer _modelRenderer;

    [SerializeField][ColorUsage(showAlpha:true, hdr:true)] Color _activeColor = Color.cyan, _unpoweredColor = Color.black;
    readonly HashSet<Enemy> _enemies = new();
    float _timer;
    bool _isActive = true, _isBright;

    static readonly int EMISSION_COLOR_ID = Shader.PropertyToID("_EmissionColor");

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
        if(!_isActive)
        {
            if(_isBright)
            {
                SetCoilMaterial(false);
            }
            return;
        }

        if(_timer < RechargeTime)
        {
            _timer += Time.deltaTime;
            if(_isBright && _enemies.Count <= 0)
            {
                SetCoilMaterial(false);
            }
        }
        else
        {
            if(_enemies.Count <= 0)
            {
                if(_isBright)
                {
                    SetCoilMaterial(false);
                }
                return;
            }

            _timer -= RechargeTime;
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
                _audioSource.Play();
            }
            if(!_isBright && hasTarget)
            {
                SetCoilMaterial(true);
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
