using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    public static event Action<bool> OnInventoryToggled;
    public static event Action<Item[]> OnLoadoutChanged;

    [Range(3, 10)][SerializeField] int _itemSlots = 10;
    [SerializeField] LoadoutSO _loadoutSO;
    [SerializeField] NewGamePlusSO _newGamePlusSO;
    [SerializeField] Item[] _allItems;
    [SerializeField] Canvas _inventoryCanvas;
    [SerializeField] Transform _layoutGroupUI;
    [SerializeField] TextMeshProUGUI _itemTitleText, _itemStatsText, _itemDescriptionText;
    [SerializeField] Image _itemImage;
    [SerializeField] InventoryItemUI _itemUIPrefab;

    readonly List<Item> _unlockedItems = new();
    readonly List<Item> _selectedItems = new();
    readonly List<InventoryItemUI> _itemUIs = new();
    InventoryItemUI _lastSelectedItemUI;

    bool _optionsEnabled, _levelOver;
    Item _selectedItem, _pointerItem;

    void Awake()
    {
        InputManager.OnInventoryPressed += ToggleInventoryCanvas;
        InventoryItemUI.OnAnyItemButtonPressed += InventoryItemUI_OnAnyItemButtonPressed;
        InventoryItemUI.OnAnyItemButtonSelected += InventoryItemUI_OnAnyItemButtonSelected;
        InventoryItemUI.OnAnyItemButtonPointedTo += InventoryItemUI_OnAnyItemButtonPointedTo;
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
                _itemUIs.Insert(index, itemUI);
                index++;
            }
        }
        _lastSelectedItemUI = _itemUIs[0];
        // TODO : Use _loadoutSO data for previously arranged items and update their .Index (also load saved data into _loadoutSO at game start)
    }

    void OnDestroy()
    {
        InputManager.OnInventoryPressed -= ToggleInventoryCanvas;
        InventoryItemUI.OnAnyItemButtonPressed -= InventoryItemUI_OnAnyItemButtonPressed;
        InventoryItemUI.OnAnyItemButtonSelected -= InventoryItemUI_OnAnyItemButtonSelected;
        InventoryItemUI.OnAnyItemButtonPointedTo -= InventoryItemUI_OnAnyItemButtonPointedTo;
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

    void InventoryItemUI_OnAnyItemButtonSelected(InventoryItemUI itemUI)
    {
        _lastSelectedItemUI = itemUI;
        _selectedItem = itemUI.Item;
        SetItemTexts(_selectedItem);
    }

    void InventoryItemUI_OnAnyItemButtonPointedTo(Item item, bool hasPointer)
    {
        _pointerItem = hasPointer ? item : null;
        if(_pointerItem)
        {
            SetItemTexts(_pointerItem);
        }
        else
        {
            SetItemTexts(_selectedItem);
        }
    }

    void SetItemTexts(Item item)
    {
        if(!item) { return; }

        _itemImage.sprite = item.Icon;
        _itemTitleText.text = item.ItemName;
        _itemDescriptionText.text = item.Description;
        if(item.IsTrap)
        {
            BuyableTrap trap = (BuyableTrap)item;
            _itemStatsText.text = $"Damage : {trap.Trap.Damage}\nCost : {trap.Cost}";
        }
        else
        {
            _itemStatsText.text = $"Cost : {item.Cost}";
        }
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool optionsEnabled)
    {
        _optionsEnabled = optionsEnabled;
        if(!optionsEnabled && _inventoryCanvas.enabled)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_lastSelectedItemUI.gameObject);
        }
    }

    void SetLevelOver()
    {
        _levelOver = true;
    }

    public void ToggleInventoryCanvas()
    {
        if(_optionsEnabled || _levelOver) { return; }

        _inventoryCanvas.enabled = !_inventoryCanvas.enabled;
        if(_inventoryCanvas.enabled)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_lastSelectedItemUI.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        OnInventoryToggled?.Invoke(_inventoryCanvas.enabled);
    }
}
