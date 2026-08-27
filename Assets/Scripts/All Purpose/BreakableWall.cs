using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] GameObject _baseModel, _shatteredVersion;
    [SerializeField] Collider _collider;
    [SerializeField] Rigidbody[] _rigidbodies;
    [SerializeField] float _forceMultiplyer = 2.25f, _destructionDelay = 5f;
    [SerializeField] GameObject _minimapIcon;

    static readonly string RAGDOLL_TAG = "Ragdoll";

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(Trap.ENEMY_TAG) || collision.gameObject.CompareTag(RAGDOLL_TAG))
        {
            _collider.enabled = false;
            _baseModel.SetActive(false);
            _shatteredVersion.SetActive(true);

            float mass = collision.rigidbody ? collision.rigidbody.mass : 1f;

            foreach(Rigidbody rigidbody in _rigidbodies)
            {
                rigidbody.AddForce(_forceMultiplyer * mass * (rigidbody.position - collision.transform.position).normalized, ForceMode.Impulse);
                rigidbody.AddTorque(Random.insideUnitSphere * Random.Range(0.5f, 2f));
            }

            Destroy(_shatteredVersion, _destructionDelay);
            if(_minimapIcon)
            {
                _minimapIcon.SetActive(false);
            }
        }
    }
}
