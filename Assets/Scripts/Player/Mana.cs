using System;
using UnityEngine;

public class Mana : MonoBehaviour
{
    public event Action<int, int> OnManaChanged;

    [SerializeField] int _maxMana = 225, _manaPerSecond = 1;

    float _regenTimer;
    int _currentMana;

    void Start()
    {
        _currentMana = _maxMana;

        OnManaChanged?.Invoke(_currentMana, _maxMana);
    }

    void Update()
    {
        if(_currentMana < _maxMana)
        {
            _regenTimer += Time.deltaTime;

            if(_regenTimer > 1)
            {
                _regenTimer -= 1;
                GainMana(_manaPerSecond);
            }
        }
    }

    void SpendMana(int amount)
    {
        _currentMana = Mathf.Max(_currentMana - amount, 0);

        OnManaChanged?.Invoke(_currentMana, _maxMana);
    }

    public bool CanAfford(int amount)
    {
        if(_currentMana < amount) { return false; }

        SpendMana(amount);
        return true;
    }

    public void GainMana(int gainedAmount)
    {
        _currentMana = Mathf.Min(_currentMana + gainedAmount, _maxMana);

        OnManaChanged?.Invoke(_currentMana, _maxMana);

        // TODO : (Blue/Cyan) Floating Text
    }

    public void ResetMana()
    {
        GainMana(_maxMana);
    }
}
