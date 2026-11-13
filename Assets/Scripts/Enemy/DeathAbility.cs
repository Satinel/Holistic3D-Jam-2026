using UnityEngine;

public class DeathAbility : MonoBehaviour
{
    [SerializeField] float _sapDuration = 3.75f;
    [SerializeField] float _sapRadius = 10f;
    [SerializeField] Health _myHealth;
    [SerializeField] Sapper _sapperPrefab;

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
        Sapper sapper = Instantiate(_sapperPrefab, transform.position, Quaternion.identity);
        sapper.Initialize(_sapDuration, _sapRadius, transform);
    }
}
