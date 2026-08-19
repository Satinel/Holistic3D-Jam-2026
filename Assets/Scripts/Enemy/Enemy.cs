using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float _ragdollRecoveryTime = 2.5f, _falloffFadeOut = 3f;
    [SerializeField] Health _health;
    [SerializeField] Collider _mainCollider;
    [SerializeField] Rigidbody _mainRigidbody;
    [SerializeField] Animator _animator;

    [SerializeField] Rigidbody[] _rigidbodies;
    bool _isRagdolled;
    float _ragddollTimer, _ragdollDuration;

    void OnCollisionEnter(Collision collision)
    {
        if(!collision.gameObject.CompareTag("Trap")) { return; }
        // TODO : Have an component which specifically causes ragdolling and supplies a RecoveryTime as well
        _ragdollDuration += _ragdollRecoveryTime;

        if(_isRagdolled) { return; }

        float mass = collision.rigidbody ? collision.rigidbody.mass : 1;

        Ragdoll(collision.contacts[0], collision.relativeVelocity * mass);
    }

    void Update()
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

        Move();
    }

    void Move()
    {
        // TODO Move using _mainRigidbody
    }

    void Ragdoll(ContactPoint contactPoint, Vector3 force)
    {
        if(_isRagdolled) { return; }

        _isRagdolled = true;
        _animator.enabled = false;

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
    }

    void RecoverFromRagdoll()
    {
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        _animator.enabled = true;
        // TODO : Set to idle animation with a long blend (if possible) to hopefully have a somewhat natural standing up kind of animation
        _isRagdolled = false;
        _ragdollDuration = 0;
        _ragddollTimer = 0;
    }
}
