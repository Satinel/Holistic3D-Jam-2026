using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [field:SerializeField] public Transform[] LanePositions { get; private set; }

    [SerializeField] Waypoint _nextWaypoint;

    void OnTriggerEnter(Collider other)
    {
        if(_nextWaypoint.LanePositions.Length <= 0) { return; }
        if(!other.CompareTag(Trap.ENEMY_TAG)) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            enemy.SetWaypointDestination(_nextWaypoint.LanePositions);
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            detector.ThisEnemy.SetWaypointDestination(_nextWaypoint.LanePositions);
        }
    }

    public void SetEnemyDestination(Enemy enemy)
    {
        enemy.SetWaypointDestination(_nextWaypoint.LanePositions);
    }
}
