using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnAnyEnemySpawned, OnAnyEnemyDestroyed;

    [field:SerializeField] public int CoreValue { get; private set; } = 1;

    [SerializeField] float _moveSpeed = 2.25f, _acceleration = 10f, _turnSpeed = 7.5f, _destroyDelay = 3f; //_deceleration = 5f;
    [SerializeField] float _ragdollRecoveryTime = 2.5f, _falloffFadeOut = 3f;
    [SerializeField] Health _health;
    [SerializeField] Collider _mainCollider;
    [SerializeField] Rigidbody _mainRigidbody;
    [SerializeField] Animator _animator;

    [SerializeField] Rigidbody _ragdoll;
    [SerializeField] ModelAnimator _ragdollModel;
    [SerializeField] Collider[] _colliders;
    [SerializeField] Rigidbody[] _rigidbodies;
    bool _isRagdolled, _isCrushed;
    float _ragddollTimer, _ragdollDuration, _crushedTimer, _startingScaleY;
    Transform _destination;

    public Health Health => _health;
    static readonly int DEATH_HASH = Animator.StringToHash("Death");


    void Awake()
    {
        _health.OnDeath += OnDeath;
        OnAnyEnemySpawned?.Invoke(this);
    }

    void OnDestroy()
    {
        OnAnyEnemyDestroyed?.Invoke(this);
        _health.OnDeath -= OnDeath;
    }

    void Start()
    {
        _startingScaleY = _ragdollModel.transform.localScale.y;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(_health.IsDead) { return; }
        if(!collision.gameObject.CompareTag("Trap")) { return; }
        if(collision.gameObject.GetComponent<Trap>()) { return; }   // This should only handle objects without attached Trap monobehaviours (like projectiles)

        float mass = collision.rigidbody ? collision.rigidbody.mass : 1;

        // Ragdoll(collision.contacts[0], collision.relativeVelocity * mass);
        Ragdoll(collision.GetContact(0), collision.relativeVelocity * mass);
    }

    void FixedUpdate()
    {
        if(_health.IsDead) { return; }

        if(_isRagdolled)
        {
            _ragddollTimer += Time.deltaTime;

            if(_ragddollTimer >= _ragdollDuration)
            {
                RecoverFromRagdoll();
            }
        }

        if(_isCrushed)
        {
            _crushedTimer -= Time.deltaTime;

            if(_crushedTimer <= 0)
            {
                RecoverFromCrushed();
            }
        }

        Move();
    }

    void Move()
    {
        if(_isRagdolled || _isCrushed) { return; }
        if(!_destination) { return; }

        RotateTowardDestination();

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float forwardVelocity = Vector3.Dot(_mainRigidbody.linearVelocity, forward);

        float speedDifference = _moveSpeed - forwardVelocity;
        _mainRigidbody.AddForce(speedDifference * _acceleration * forward, ForceMode.Acceleration);
    }

    void RotateTowardDestination()
    {
        if(!_destination) { return; }

        Vector3 direction = _destination.position - transform.position;
        direction.y = 0;
        Vector3 rotationToFace = direction.normalized;

        if(rotationToFace.sqrMagnitude > 0.001f)
        {
            _mainRigidbody.MoveRotation(Quaternion.Slerp(_mainRigidbody.rotation, Quaternion.LookRotation(rotationToFace, Vector3.up), _turnSpeed * Time.deltaTime));
        }
    }

    void Ragdoll(ContactPoint contactPoint, Vector3 force)
    {
        _ragdollDuration = _ragdollDuration < _ragdollRecoveryTime ? _ragdollRecoveryTime : _ragdollDuration;
        _ragddollTimer = 0;

        if(_isRagdolled) { return; }

        _isRagdolled = true;
        _animator.enabled = false;
        _mainCollider.enabled = false;
        _mainRigidbody.isKinematic = true;

        Rigidbody closestBone = null;
        float smallestDistance = Mathf.Infinity;

        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;

            float distance = (rigidbody.worldCenterOfMass - contactPoint.point).sqrMagnitude;
            closestBone = distance < smallestDistance ? rigidbody : closestBone;
        }

        if(closestBone != null)
        {
            closestBone.AddForceAtPosition(force, contactPoint.point, ForceMode.Impulse);

            foreach(Rigidbody rigidbody in _rigidbodies)
            {
                if(rigidbody == closestBone) { continue; }

                float distance = Vector3.Distance(rigidbody.worldCenterOfMass, contactPoint.point);
                float falloff = Mathf.Clamp01(1f - distance / _falloffFadeOut);
                rigidbody.AddForceAtPosition(force * falloff, contactPoint.point, ForceMode.Impulse);
            }
        }

        foreach(Collider collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    public void AccurateRagdoll(Vector3 force, ForceMode forceMode, float ragdollDuration)
    {
        _ragdollDuration = ragdollDuration > _ragdollDuration ? ragdollDuration : _ragdollDuration;
        _ragddollTimer = 0;
        _isRagdolled = true;
        _animator.enabled = false;
        _mainCollider.enabled = false;
        _mainRigidbody.isKinematic = true;

        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(force, forceMode);
        }
        foreach(Collider collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    void RecoverFromRagdoll()
    {
        Vector3 ragdollPosition = _ragdoll.position;
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        foreach(Collider collider in _colliders)
        {
            collider.enabled = false;
        }
        _mainRigidbody.position = ragdollPosition;
        // TODO : Check if this position is inside a non-trigger collider and move it out if so (otherwise Enemies get sucked through walls)
        _mainCollider.enabled = true;
        _mainRigidbody.isKinematic = false;

        if(!_isCrushed)
        {
            _animator.enabled = true;
        }
        // TODO (but probably won't have time in a game jam) : Set to a stand up animation based on supine/prone position

        _isRagdolled = false;
        _ragdollDuration = 0;
        _ragddollTimer = 0;
    }

    public void Crush(float newScaleY, float duration)
    {
        if(_isRagdolled) { return; }

        _animator.enabled = false;
        _crushedTimer = duration;
        _ragdollModel.transform.localScale = new(_ragdollModel.transform.localScale.x, newScaleY, _ragdollModel.transform.localScale.z);
        _isCrushed = true;
    }

    void RecoverFromCrushed()
    {
        _ragdollModel.transform.localScale = new(_ragdollModel.transform.localScale.x, _startingScaleY, _ragdollModel.transform.localScale.z);
        // TODO : Maybe play Pop sound effect from 2D Princess here
        _isCrushed = false;
        if(!_isRagdolled)
        {
            _animator.enabled = true;
        }
    }

    public void DisableRagdollGravity()
    {
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.useGravity = false;
        }
    }

    public void SetDestination(Transform destination)
    {
        _destination = destination;
    }

    void OnDeath()
    {
        if(!_isRagdolled)
        {
            _animator.SetTrigger(DEATH_HASH);
        }

        // A really fancy shader should make the model disintegrate or something!!!
        Destroy(gameObject, _destroyDelay);
    }
}
