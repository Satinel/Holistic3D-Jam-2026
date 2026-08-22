using UnityEngine;

public class Enemy : MonoBehaviour
{
    [field:SerializeField] public int CoreValue { get; private set; } = 1;

    [SerializeField] float _moveSpeed = 2.25f, _acceleration = 10f, _deceleration = 5f, _turnSpeed = 7.5f, _destroyDelay = 3f;
    [SerializeField] float _ragdollRecoveryTime = 2.5f, _falloffFadeOut = 3f;
    [SerializeField] Health _health;
    [SerializeField] Collider _mainCollider;
    [SerializeField] Rigidbody _mainRigidbody;
    [SerializeField] Animator _animator;

    [SerializeField] Rigidbody _ragdoll;
    [SerializeField] Collider[] _colliders;
    [SerializeField] Rigidbody[] _rigidbodies;
    bool _isRagdolled;
    float _ragddollTimer, _ragdollDuration;
    Transform _destination;

    static readonly int DEATH_HASH = Animator.StringToHash("Death");


    void Awake()
    {
        _health.OnDeath += OnDeath;
    }

    void OnDestroy()
    {
        _health.OnDeath -= OnDeath;
    }

    void OnCollisionEnter(Collision collision)  // Note : Once Ragdoll() is called, _mainCollider (and thus this method) is disabled
    {
        if(!collision.gameObject.CompareTag("Trap")) { return; }

        if(collision.gameObject.TryGetComponent(out Trap trap))
        {
            if(trap.Damage > 0)
            {
                _health.LoseHealth(trap.Damage);
            }

            if(!trap.UsesPhysics)
            {
                trap.TrapAction(this);
                return;
            }

            _ragdollDuration += trap.RagdollDuration;
        }
        else
        {
            _ragdollDuration += _ragdollRecoveryTime;
        }

        if(_isRagdolled) { return; }    // Note : This entire method will never be called while isRagdolled == true because the collider is disabled, so...

        if(trap && trap.OverridesPhysics)   // TODO : Overriding physics while isRagdolled requires code attached to each collider in the ragdoll itself, it can't be done here
        {
            Ragdoll(collision.contacts[0], trap.DirectionOverride * trap.ForceOverride, trap.ForceMode);
            return;
        }

        float mass = collision.rigidbody ? collision.rigidbody.mass : 1;

        Ragdoll(collision.contacts[0], collision.relativeVelocity * mass);
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
                return;
            }
        }

        Move();
    }

    void Move()
    {
        if(!_destination) { return; }

        RotateTowardDestination();

        float forwardVelocity = Vector3.Dot(_mainRigidbody.linearVelocity, transform.forward);

        if(forwardVelocity < _moveSpeed)
        {
            _mainRigidbody.AddForce(transform.forward * _acceleration, ForceMode.Force);
        }
        else if(forwardVelocity > _moveSpeed)
        {
            _mainRigidbody.AddForce(-transform.forward * _deceleration, ForceMode.Force);
        }
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

    void Ragdoll(ContactPoint contactPoint, Vector3 force, ForceMode forceMode = ForceMode.Impulse)
    {
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
            closestBone.AddForceAtPosition(force, contactPoint.point, forceMode);

            foreach(Rigidbody rigidbody in _rigidbodies)
            {
                if(rigidbody == closestBone) { continue; }

                float distance = Vector3.Distance(rigidbody.worldCenterOfMass, contactPoint.point);
                float falloff = Mathf.Clamp01(1f - distance / _falloffFadeOut);
                rigidbody.AddForceAtPosition(force * falloff, contactPoint.point, forceMode);
            }
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

        _animator.enabled = true;
        // TODO (but probably won't have time in a game jam) : Set to a stand up animation based on supine/prone position

        _isRagdolled = false;
        _ragdollDuration = 0;
        _ragddollTimer = 0;
    }

    public void ChangeRagdollGravity(bool enabled)
    {
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.useGravity = enabled;
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
