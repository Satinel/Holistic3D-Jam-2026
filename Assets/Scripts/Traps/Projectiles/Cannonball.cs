using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [field:SerializeField] public Rigidbody Rigidbody { get; private set; }

    [SerializeField] int _damage = 10, _penetration = 5;
    [SerializeField] bool _destroyOnImpact, _usePenetration;
    [SerializeField] float _destructionDelay = 1.75f;

    void OnCollisionEnter(Collision collision)
    {
        if(_destroyOnImpact)
        {
            Destroy(gameObject, 0.1f);
        }

        if(collision.gameObject.TryGetComponent(out Health health))
        {
            if(health.IsPlayer) { return; } // This shouldn't ever happen but there's nothing wrong with making sure

            if(_damage > 0)
            {
                health.LoseHealth(_damage);
            }

            _penetration = _usePenetration ? _penetration -1 : _penetration;

            if(_penetration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(int damage)
    {
        _damage = damage;

        Destroy(gameObject, _destructionDelay);
    }
}
