using UnityEngine;

public class Shove : Item
{
    [SerializeField] int _secondaryDamage = 10;
    [SerializeField] float _forceMultiplyer = 25f, _secondaryforceMult = 50f;
    [SerializeField] Transform _spawnPoint, _aimPositionMarker;
    [SerializeField] Cannonball _cannonballPrefab;
    [SerializeField] Wave _shoveWavePrefab;

    public override void PrimaryAction()
    {
        Cannonball cannonball = Instantiate(_cannonballPrefab, _spawnPoint.position, _spawnPoint.rotation);
        cannonball.transform.forward = (_aimPositionMarker.position - cannonball.transform.position).normalized;
        cannonball.Initialize(Damage);
        cannonball.Rigidbody.AddForce(cannonball.transform.forward * _forceMultiplyer, ForceMode.Impulse);
    }

    public override void SecondaryAction()
    {
        base.SecondaryAction();
        Wave shoveWave = Instantiate(_shoveWavePrefab, _spawnPoint.position, _spawnPoint.rotation);
        shoveWave.transform.forward = (_aimPositionMarker.position - shoveWave.transform.position).normalized;
        shoveWave.Initialize(_secondaryDamage);
        shoveWave.Rigidbody.AddForce(shoveWave.transform.forward * _secondaryforceMult, ForceMode.Impulse);
    }
}
