using UnityEngine;

public class Payload : MonoBehaviour
{
    [SerializeField] Trap _parentTrap;

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(Trap.ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            _parentTrap.HitEnemy(enemy);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            _parentTrap.HitEnemy(detector.ThisEnemy);
        }
    }
}
