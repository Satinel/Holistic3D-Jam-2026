using System;
using UnityEngine;

public class BarricadeTrap : Trap
{
    public static event Action<BarricadeTrap> OnAnyBarricadePlaced, OnAnyBarricadeDestroyed;

    [field:SerializeField] public Transform[] AttackPoints { get; private set; }

    int _votesToDestroy;
    Waypoint _intersectingWaypoint;

    protected override void Awake() // Currently there's no reason to override and include base.Awake() but the logic could be relevant later
    {
        base.Awake();
        OnAnyBarricadePlaced?.Invoke(this);
    }

    protected override void OnDestroy() // As with Awake, no need to override and base.OnDestroy() it's just a safety precaution if base changes
    {
        if(_trapSocket)
        {
            _trapSocket.TrapDestroyed();
        }
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

        if(!other.CompareTag(ENEMY_TAG)) { return; }

        Enemy detectedEnemy = null;

        if(other.TryGetComponent(out Enemy enemy))
        {
            detectedEnemy = enemy;
        }
        else if(other.TryGetComponent(out WaypointDetector detector))
        {
            detectedEnemy = detector.ThisEnemy;
        }

        if(detectedEnemy == null || detectedEnemy.EnemyHealth.IsDead) { return; }

        if(_votesToDestroy > 0 && enemy)
        {
            enemy.AttackBarricade(this);
        }
        else if(_intersectingWaypoint)
        {
            _intersectingWaypoint.SetEnemyDestination(detectedEnemy);
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
GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
marker.transform.position = transform.position;
marker.GetComponent<Collider>().enabled = false;
        // TODO Give Barricades a health value and have enemies destroy them after several attacks instead of instantly on contact
Debug.Log("Destroyed by " + attacker.name);
        Destroy(gameObject);
    }
}
