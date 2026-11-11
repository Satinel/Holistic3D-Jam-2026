using UnityEngine;

public class Sapper : MonoBehaviour
{
    [SerializeField] float _sapDuration = 3.75f;
    [SerializeField] float _sapRadius = 10f;
    [SerializeField] Health _myHealth;

    void Awake()
    {
        _myHealth.OnDeath += Health_OnDeath;
    }

    void OnDestroy()
    {
        _myHealth.OnDeath -= Health_OnDeath;
    }

    void Health_OnDeath()
    {
        Collider[] traps = Physics.OverlapSphere(transform.position, _sapRadius);   // TODO ? Replace with instantiating a sphere collider at transform.position which uses OnTriggerEnter (plus particle system)

        foreach(Collider trap in traps)
        {
            if(trap.TryGetComponent(out ISapable sapable))
            {
                sapable.Sap(_sapDuration);
            }
        }
    }
}
