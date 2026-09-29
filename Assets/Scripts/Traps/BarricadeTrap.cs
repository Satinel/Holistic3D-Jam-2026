using System;
using UnityEngine;

public class BarricadeTrap : Trap
{
    public static event Action<BarricadeTrap> OnAnyBarricadePlaced, OnAnyBarricadeDestroyed;

    public bool ShouldDestroy { get; private set; }

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
        if(!ShouldDestroy) { return; }

        if(other.TryGetComponent(out WaypointDetector detector))
        {
            detector.ThisEnemy.AttackBarricade(this);
        }
    }

    public void SetShouldDestroy()
    {
        ShouldDestroy = true;
Debug.Log(name + " is set to be destroyed for blocking the way!");
    }

    public void SufferAttack(Enemy attacker)
    {
        // TODO Handle this better (assuming any of this even works...)
Debug.Log("Destroyed by " + attacker.name);
        Destroy(gameObject);
    }
}
