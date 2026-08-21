using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public event Action<int> OnMoneyChanged;

    [SerializeField] int _startingMoney = 500;

    int _money;

    void Awake()
    {
        Health.OnAnyHealthDeath += Health_OnAnyHealthDeath;
        TrapSocket.OnAnyTrapSold += TrapSocket_OnAnyTrapSold;
    }

    void OnDestroy()
    {
        Health.OnAnyHealthDeath -= Health_OnAnyHealthDeath;
        TrapSocket.OnAnyTrapSold -= TrapSocket_OnAnyTrapSold;
    }

    void Start()
    {
        _money = _startingMoney;
        OnMoneyChanged?.Invoke(_money);
    }

    public void GainMoney(int value)
    {
        _money += value;

        OnMoneyChanged?.Invoke(_money);
    }

    public bool CanAfford(int price)
    {
        return _money >= price;
    }

    public void SpendMoney(int price)
    {
        _money -= price;

        OnMoneyChanged?.Invoke(_money);
        // TODO Buy things
    }

    void Health_OnAnyHealthDeath(Health health)
    {
        if(health.IsPlayer) { return; } // No earning money through player deaths!

        GainMoney(health.MoneyValue);
    }

    void TrapSocket_OnAnyTrapSold(int soldPrice)
    {
        GainMoney(soldPrice);
    }
}
