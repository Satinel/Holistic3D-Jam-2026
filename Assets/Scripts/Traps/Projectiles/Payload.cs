using UnityEngine;

public class Payload : MonoBehaviour
{
    [SerializeField] Trap _parentTrap;

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(Trap.ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            if(enemy.Health.IsDead) { return; }

            _parentTrap.GetForceDirection((other.transform.position - transform.position).normalized);
            _parentTrap.HitEnemy(enemy);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            if(detector.ThisEnemy.Health.IsDead) { return; }

            _parentTrap.GetForceDirection((other.transform.position - transform.position).normalized);
            _parentTrap.HitEnemy(detector.ThisEnemy);
        }
    }
}
