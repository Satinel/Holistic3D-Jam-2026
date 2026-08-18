using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    public event Action<int> OnMoneyChanged;

    int _money;

    void Start()
    {
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
}
