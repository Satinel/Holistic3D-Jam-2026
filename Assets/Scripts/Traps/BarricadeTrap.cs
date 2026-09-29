using System;
using UnityEngine;

public class BarricadeTrap : Trap
{
    public static event Action<BarricadeTrap> OnAnyBarricadePlaced, OnAnyBarricadeDestroyed;

    int _votesToDestroy;
    Waypoint _intersectingWaypoint;

    protected override void Awake() // Currently there's no reason to override and include base.Awake() but the logic could be relevant later
    {
        base.Awake();
        OnAnyBarricadePlaced?.Invoke(this);
    }

    protected override void OnDestroy() // As with Awake, no need to override and base.OnDestroy() it's just a safety precaution if base changes
    {
        OnAnyBarricadeDestroyed?.Invoke(this);
        base.OnDestroy();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Waypoint waypoint))
        {
            _intersectingWaypoint = waypoint;
            return;
        }

        if(!_intersectingWaypoint && _votesToDestroy <= 0) { return; }

        if(other.TryGetComponent(out WaypointDetector detector))
        {
            if(_votesToDestroy > 0)
            {
                detector.ThisEnemy.AttackBarricade(this);
            }
            else if(_intersectingWaypoint)
            {
                _intersectingWaypoint.SetEnemyDestination(detector.ThisEnemy);
            }
        }
    }

    public void VoteToDestroy()
    {
        _votesToDestroy++;
Debug.Log($"{name} has {_votesToDestroy} votes to be destroyed for blocking the way!");
    }

    public void VoteToIgnore()
    {
        _votesToDestroy = Mathf.Max(0, _votesToDestroy - 1);
Debug.Log($"{name} has {_votesToDestroy} votes after lobbying not to be destroyed!");
    }

    public void SufferAttack(Enemy attacker)
    {
        // TODO Handle this better (assuming any of this even works...)
Debug.Log("Destroyed by " + attacker.name);
        Destroy(gameObject);
    }
}
