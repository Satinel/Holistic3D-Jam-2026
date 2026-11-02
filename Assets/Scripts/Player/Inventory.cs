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
    [SerializeField] Canvas _inventoryCanvas;   // TODO : Input to enable/disable (and use TimeManager to pause/unpause game)
    [SerializeField] Transform _layoutGroupUI;
    [SerializeField] InventoryItemUI _itemUIPrefab;

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
                InventoryItemUI itemUI = Instantiate(_itemUIPrefab, _layoutGroupUI);
                itemUI.Initialize(_allItems[i], index);
                index++;
            }
        }
        _selectedItems = _unlockedItems.ToArray();
        // TODO : Use _loadoutSO data for previously arranged _selectedItems (also load saved data into _loadoutSO at game start)
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
