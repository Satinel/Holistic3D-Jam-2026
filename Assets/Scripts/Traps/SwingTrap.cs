using UnityEngine;

public class SwingTrap : Trap
{
    [SerializeField] Transform _swing;
    [SerializeField] float _forceMultiplyer = 15f, _swingSpeed = 50f, _maxRotation = 270f, _minRotation = 90f, _sfxDelay = 0.9f;
    [SerializeField] AudioClip[] _hitSFX;

    float _timer, _sfxTimer;
    bool _isIncreasing, _isPaused;
    Vector3 _forceDirection = new();

    void Update()
    {
        if(_sfxTimer > 0)
        {
            _sfxTimer = Mathf.Max(0, _sfxTimer - Time.deltaTime);
        }

        if(_isPaused)
        {
            _timer += Time.deltaTime;

            if(_timer > RechargeTime)
            {
                _isPaused = false;
                _audioSource.Play();
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
        if(_sfxTimer <= 0)
        {
            _audioSource.PlayOneShot(_hitSFX[Random.Range(0, _hitSFX.Length)]);
            _sfxTimer = _sfxDelay;
        }

        enemy.AccurateRagdoll(_forceDirection * _forceMultiplyer, ForceMode, RagdollDuration);
        if(Damage > 0)
        {
            enemy.EnemyHealth.LoseHealth(Damage);
        }
    }
}
