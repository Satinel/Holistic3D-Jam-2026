using System;
using UnityEngine;

public class Core : MonoBehaviour
{
    public static event Action<int> OnCoreValueChanged;
    public static event Action OnCoreValueLowered;
    public static event Action OnCoreDestroyed;

    [SerializeField] int _maxCharge;
    [SerializeField] float _enemyDestructionDelay = 1.25f;
    [SerializeField] Collider _collider;
    [SerializeField] AudioSource _audioSource;

    int _currentCharge;
    bool _coreDestroyed;

    void Awake()
    {
        Health.OnAnyHealthDeath += Health_OnAnyHealthDeath;
    }

    void OnDestroy()
    {
        Health.OnAnyHealthDeath -= Health_OnAnyHealthDeath;
    }

    void Start()
    {
        _currentCharge = _maxCharge;
        OnCoreValueChanged?.Invoke(_currentCharge);
    }

    void OnTriggerEnter(Collider other)
    {
        if(_coreDestroyed) { return; }

        if(other.TryGetComponent(out Enemy enemy))
        {
            LowerCoreCharge(enemy.CoreValue);
            Destroy(enemy.gameObject, _enemyDestructionDelay);
        }
    }

    void LowerCoreCharge(int amount)
    {
        if(_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        _audioSource.Play();

        _currentCharge -= amount;
        _currentCharge = _currentCharge < 0 ? 0 : _currentCharge;

        OnCoreValueLowered?.Invoke();
        OnCoreValueChanged?.Invoke(_currentCharge);

        if(_currentCharge == 0)
        {
            _coreDestroyed = true;
            _collider.enabled = false;
            OnCoreDestroyed?.Invoke();
        }
    }

    void Health_OnAnyHealthDeath(Health health)
    {
        if(!health.IsPlayer) { return; }

        LowerCoreCharge(5); // Note : 5 is arbitrary but probably fine, if the player even CAN die by jam time
    }
}
