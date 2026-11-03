using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static event Action<bool> OnInventoryToggled;
    public static event Action<Item[]> OnLoadoutChanged;

    [Range(3, 10)][SerializeField] int _itemSlots = 10;
    [SerializeField] LoadoutSO _loadoutSO;
    [SerializeField] NewGamePlusSO _newGamePlusSO;
    [SerializeField] Item[] _allItems;
    [SerializeField] Canvas _inventoryCanvas;   // TODO : Input to enable/disable (and use TimeManager to pause/unpause game)
    [SerializeField] Transform _layoutGroupUI;
    [SerializeField] InventoryItemUI _itemUIPrefab;

    readonly List<Item> _unlockedItems = new();
    readonly List<Item> _selectedItems = new();

    bool _optionsEnabled, _levelOver;

    void Awake()
    {
        InputManager.OnInventoryPressed += ToggleInventoryCanvas;
        InventoryItemUI.OnAnyItemButtonPressed += InventoryItemUI_OnAnyItemButtonPressed;
        OptionsMenu.OnOptionsCanvasToggled += OptionsMenu_OnOptionsCanvasToggled;
        LevelManager.OnLevelCompleted += SetLevelOver;
        LevelManager.OnLevelFailed += SetLevelOver;

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
        // TODO : Use _loadoutSO data for previously arranged items and update their .Index (also load saved data into _loadoutSO at game start)
    }

    void OnDestroy()
    {
        InputManager.OnInventoryPressed -= ToggleInventoryCanvas;
        InventoryItemUI.OnAnyItemButtonPressed -= InventoryItemUI_OnAnyItemButtonPressed;
        OptionsMenu.OnOptionsCanvasToggled -= OptionsMenu_OnOptionsCanvasToggled;
        LevelManager.OnLevelCompleted -= SetLevelOver;
        LevelManager.OnLevelFailed -= SetLevelOver;
    }

    void Start()
    {
        if(_newGamePlusSO && _newGamePlusSO.IsNewGamePlus)
        {
            _itemSlots = 10;
        }

        _selectedItems.Clear();

        for(int i = 0; i < _itemSlots; i++)
        {
            _selectedItems.Insert(i, _unlockedItems[i]);
        }

        if(_selectedItems.Count > 0)
        {
            OnLoadoutChanged?.Invoke(_selectedItems.ToArray());
        }
    }

    void InventoryItemUI_OnAnyItemButtonPressed(InventoryItemUI itemUI)
    {
        if(itemUI.Item.Index < 0)
        {
            // itemUI and Item would (somehow) not be initialized properly in this case
            return;
        }
        if(!_selectedItems.Contains(itemUI.Item))
        {
            if(_selectedItems.Count >= _itemSlots)
            {
                // TODO : Play error sound because item can't be added to a filled hotbar
                return;
            }

            itemUI.Item.SetIndex(_selectedItems.Count);   // This has to happen before the .Insert or else the new index will be off by one
            _selectedItems.Insert(_selectedItems.Count, itemUI.Item);
        }
        else
        {
            if(itemUI.Item.WasUsed)
            {
                // TODO : Play error sound because item can't be removed after use
                return;
            }

            int changedIndex = itemUI.Item.Index;

            _selectedItems.RemoveAt(changedIndex);
            for(int i = changedIndex; i < _selectedItems.Count; i++)
            {
                _selectedItems[i].SetIndex(i);
            }
        }
        OnLoadoutChanged?.Invoke(_selectedItems.ToArray());
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool optionsEnabled)
    {
        _optionsEnabled = optionsEnabled;
    }

    void SetLevelOver()
    {
        _levelOver = true;
    }

    public void ToggleInventoryCanvas()
    {
        if(_optionsEnabled || _levelOver) { return; }

        _inventoryCanvas.enabled = !_inventoryCanvas.enabled;
        OnInventoryToggled?.Invoke(_inventoryCanvas.enabled);
    }
}
