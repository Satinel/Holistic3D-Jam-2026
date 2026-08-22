using UnityEngine;

public class Shove : Item
{
    [SerializeField] int _damage = 0;
    [SerializeField] float _forceMultiplyer = 25f;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] Cannonball _cannonballPrefab;

    public override void PrimaryAction(Vector3 direction)
    {
        Cannonball cannonball = Instantiate(_cannonballPrefab, _spawnPoint.position, _spawnPoint.rotation);
        cannonball.transform.forward = (direction - cannonball.transform.position).normalized;
        cannonball.Initialize(_damage);
        cannonball.Rigidbody.AddForce(cannonball.transform.forward * _forceMultiplyer, ForceMode.Impulse);
    }
}
