using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] Waypoint[] _nextWaypoints;

    void OnTriggerEnter(Collider other)
    {
        if(_nextWaypoints.Length <= 0) { return; }
        if(!other.CompareTag(Trap.ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            enemy.SetDestination(GetNextWaypoint());
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            detector.ThisEnemy.SetDestination(GetNextWaypoint());
        }
    }

    Transform GetNextWaypoint()
    {
        return _nextWaypoints[Random.Range(0, _nextWaypoints.Length)].transform;
    }
}
