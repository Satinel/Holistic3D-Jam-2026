using UnityEngine;

public class PushTrap : Trap
{
    [SerializeField] Collider _triggerCollider, _physicsCollider;
    [SerializeField] Animator _animator;

    static readonly int TRIGGER_HASH = Animator.StringToHash("Trigger");

    float _timer;

    void Start()
    {
        _physicsCollider.enabled = false;
        _timer = RechargeTime;
        _triggerCollider.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(ENEMY_TAG))
        {
            Trigger();
        }
    }

    void FixedUpdate()
    {
        if(_timer < RechargeTime)
        {
            _timer = _timer + Time.deltaTime >= RechargeTime ? RechargeTime : _timer + Time.deltaTime;

            if(_timer == RechargeTime)
            {
                _triggerCollider.enabled = true;
            }
        }
    }

    void Trigger()
    {
        _triggerCollider.enabled = false;
        _physicsCollider.enabled = true;
        _timer = 0;
        _animator.SetTrigger(TRIGGER_HASH);
    }

    void ActivationFinishedAnimationEvent()
    {
        _physicsCollider.enabled = false;
    }
}
