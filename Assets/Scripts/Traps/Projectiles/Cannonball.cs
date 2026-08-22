using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [field:SerializeField] public Rigidbody Rigidbody { get; private set; }

    [SerializeField] int _damage = 10;
    [SerializeField] bool _destroyOnImpact;
    [SerializeField] float _destructionDelay = 1.75f;

    void OnCollisionEnter(Collision collision)
    {
        if(_destroyOnImpact)
        {
            Destroy(gameObject);
        }

        if(_damage <= 0) { return; }

        if(collision.gameObject.TryGetComponent(out Health health))
        {
            if(health.IsPlayer) { return; } // This shouldn't ever happen but there's nothing wrong which making sure

            health.LoseHealth(_damage);
        }
    }

    public void Initialize(int damage)
    {
        _damage = damage;

        Destroy(gameObject, _destructionDelay);
    }
}
