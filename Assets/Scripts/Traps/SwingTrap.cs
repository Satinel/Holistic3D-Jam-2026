using UnityEngine;

public class SwingTrap : Trap
{
    [SerializeField] Transform _swing;
    [SerializeField] float _forceMultiplyer = 15f, _swingSpeed = 50f, _maxRotation = 270f, _minRotation = 90f;

    float _timer;
    bool _isIncreasing, _isPaused;
    Vector3 _forceDirection = new();

    void Update()
    {
        if(_isPaused)
        {
            _timer += Time.deltaTime;

            if(_timer > RechargeTime)
            {
                _isPaused = false;
            }
            else
            {
                return;
            }
        }

        if(_isIncreasing)
        {
            _swing.Rotate(_swingSpeed * Time.deltaTime * Vector3.forward);
            if(_swing.eulerAngles.z >= _maxRotation)
            {
                _isIncreasing = false;
                _timer = 0;
                _isPaused = true;
            }
        }
        else
        {
            _swing.Rotate(_swingSpeed * Time.deltaTime * -Vector3.forward);
            if(_swing.eulerAngles.z <= _minRotation)
            {
                _isIncreasing = true;
                _timer = 0;
                _isPaused = true;
            }
        }
    }

    public override void GetForceDirection(Vector3 direction)
    {
        _forceDirection = direction;
    }

    public override void HitEnemy(Enemy enemy)
    {
        enemy.AccurateRagdoll(_forceDirection * _forceMultiplyer, ForceMode, RagdollDuration);
        if(Damage > 0)
        {
            enemy.EnemyHealth.LoseHealth(Damage);
        }
    }
}
