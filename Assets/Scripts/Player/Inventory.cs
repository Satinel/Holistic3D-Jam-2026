using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static event Action<Item[]> OnLoadoutChanged;

    [Range(3, 10)][SerializeField] int _itemSlots = 10;
    [SerializeField] LoadoutSO _loadoutSO;
    [SerializeField] NewGamePlusSO _newGamePlusSO;
    [SerializeField] Item[] _allItems;

    readonly List<Item> _unlockedItems = new();
    Item[] _selectedItems;

    void Awake()
    {
        _unlockedItems.Clear();
        int index = 0;
        for(int i = 0; i < _allItems.Length; i++)
        {
            if(_allItems[i].IsUnlocked)
            {
                _unlockedItems.Insert(index, _allItems[i]);
                index++;
            }
        }
        _selectedItems = _unlockedItems.ToArray();
    }

    void Start()
    {
        if(_newGamePlusSO && _newGamePlusSO.IsNewGamePlus)
        {
            _itemSlots = 10;
        }

        if(_selectedItems.Length > _itemSlots)
        {
            Array.Resize(ref _selectedItems, _itemSlots);
        }

        if(_selectedItems.Length > 0)
        {
            OnLoadoutChanged?.Invoke(_selectedItems);
        }
    }
}
