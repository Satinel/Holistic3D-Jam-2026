using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] Waypoint[] _nextWaypoints;

    void OnTriggerEnter(Collider other)
    {
        if(_nextWaypoints.Length <= 0) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            enemy.SetDestination(GetNextWaypoint());
        }
    }

    Transform GetNextWaypoint()
    {
        return _nextWaypoints[Random.Range(0, _nextWaypoints.Length)].transform;
    }
}
