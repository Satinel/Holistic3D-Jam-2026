using UnityEngine;

public class Wave : MonoBehaviour
{
    [field:SerializeField] public Rigidbody Rigidbody { get; private set; }

    [SerializeField] int _damage = 10;
    [SerializeField] float _destructionDelay = 30f, _ragdollDuration = 5f;
    [SerializeField] GameObject _particlesPrefab;
    [SerializeField] AudioClip[] _hitSFX;

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(Trap.ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            if (enemy.EnemyHealth.IsDead) { return; }

            HitEnemy(other, enemy);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            if(detector.ThisEnemy.EnemyHealth.IsDead) { return; }

            HitEnemy(other, detector.ThisEnemy);
        }
    }

    void HitEnemy(Collider other, Enemy enemy)
    {
        AudioSource audioSource = null;

        if(_particlesPrefab)
        {
            GameObject particles = Instantiate(_particlesPrefab, other.ClosestPoint(transform.position), Quaternion.identity);

            if (_hitSFX.Length > 0)
            {
                audioSource = particles.GetComponent<AudioSource>();

                if (_hitSFX.Length > 0 && audioSource)
                {
                    audioSource.PlayOneShot(_hitSFX[Random.Range(0, _hitSFX.Length)]);
                }
            }
        }

        enemy.AccurateRagdoll(Rigidbody.linearVelocity, ForceMode.Impulse, _ragdollDuration);

        if(_damage > 0)
        {
            enemy.EnemyHealth.LoseHealth(_damage);
        }
    }

    public void Initialize(int damage)
    {
        _damage = damage;

        Destroy(gameObject, _destructionDelay);
    }
}
