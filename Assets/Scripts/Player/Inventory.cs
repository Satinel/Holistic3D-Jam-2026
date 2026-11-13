using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    public static event Action<int> AnnounceItemSlotTotal;
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
    [SerializeField] Button _closeButton;
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
        LevelManager.OnSceneChangeStarted += LevelManager_OnSceneChangeStarted;
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
        LevelManager.OnSceneChangeStarted -= LevelManager_OnSceneChangeStarted;
    }

    void Start()
    {
        if(_newGamePlusSO && _newGamePlusSO.IsNewGamePlus)
        {
            UnlockAll();
        }

        AnnounceItemSlotTotal?.Invoke(_itemSlots);

        _unlockedItems.Clear();
        int index = 0;
        for(int i = 0; i < _allItems.Length; i++)
        {
            if(_allItems[i].IsUnlocked)
            {
                _allItems[i].SetIndex(i);
                _unlockedItems.Insert(index, _allItems[i]);
                InventoryItemUI itemUI = Instantiate(_itemUIPrefab, _layoutGroupUI);
                itemUI.Initialize(_allItems[i], index);
                _itemUIs.Insert(index, itemUI);
                index++;
            }
        }
        _lastSelectedItemUI = _itemUIs[0];

        SetButtonNavigations();

        if(_loadoutSO && _loadoutSO.HasSavedData)
        {
            _selectedItems.Clear();

            for(int i = 0; i < _itemSlots; i++)
            {
                if(i >= _unlockedItems.Count) { break; }

                int slotValue = _loadoutSO.GetSlotValue(i);

                if(slotValue < 0 || slotValue > _allItems.Length) { break; }

                _selectedItems.Insert(i, _allItems[slotValue]);
                _selectedItems[i].SetItemSlot(i);
            }

            if(_selectedItems.Count > 0)
            {
                OnLoadoutChanged?.Invoke(_selectedItems.ToArray());
            }
        }
        else
        {
            _selectedItems.Clear();
            _selectedItems.Insert(0, _unlockedItems[0]);
            OnLoadoutChanged?.Invoke(_selectedItems.ToArray());
        }
    }

    void UnlockAll()
    {
        _itemSlots = 10;
        foreach(Item item in _allItems)
        {
            item.Unlock(true);
        }
    }

    void SetButtonNavigations()
    {
        Button up = _closeButton;
        Button down = _itemUIs.Count > 4 ? _itemUIs[4].Button : _closeButton;
        _itemUIs[0].SetButtonNavigation(up, down, _itemUIs[^1].Button, _itemUIs[1].Button);

        for(int i = 1; i < _itemUIs.Count - 1; i++)
        {
            down = _itemUIs.Count > i + 4 ? _itemUIs[i + 4].Button : _closeButton;
            up = i > 3 ? _itemUIs[i - 4].Button : _closeButton;
            _itemUIs[i].SetButtonNavigation(up, down, _itemUIs[i - 1].Button, _itemUIs[i + 1].Button);
        }

        up = _itemUIs.Count > 4 ? _itemUIs[^5].Button : _closeButton;
        _itemUIs[^1].SetButtonNavigation(up, _closeButton, _itemUIs[^2].Button, _itemUIs[0].Button);

        Navigation rebindNavigation = _closeButton.navigation;
        rebindNavigation.mode = Navigation.Mode.Explicit;
        rebindNavigation.selectOnUp = _itemUIs[^1].Button;
        rebindNavigation.selectOnDown = _itemUIs[0].Button;
        rebindNavigation.selectOnLeft = null;
        rebindNavigation.selectOnRight = null;
        _closeButton.navigation = rebindNavigation;
    }

    void InventoryItemUI_OnAnyItemButtonPressed(InventoryItemUI itemUI)
    {
        if(itemUI.Item.Slot < 0)
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

            itemUI.Item.SetItemSlot(_selectedItems.Count);   // This has to happen before the .Insert or else the new ItemSlot will be off by one
            _selectedItems.Insert(_selectedItems.Count, itemUI.Item);
        }
        else
        {
            if(itemUI.Item.WasUsed)
            {
                // TODO : Play error sound because item can't be removed after use
                return;
            }

            int changedSlot = itemUI.Item.Slot;

            _selectedItems.RemoveAt(changedSlot);
            for(int i = changedSlot; i < _selectedItems.Count; i++)
            {
                _selectedItems[i].SetItemSlot(i);
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
        _itemStatsText.text = $"Damage : {item.Damage}\nCost : {item.Cost}";
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

    void LevelManager_OnSceneChangeStarted()
    {
        if(_loadoutSO)
        {
            for(int i = 0; i < _itemSlots; i++)
            {
                if(i > _selectedItems.Count - 1)
                {
                    _loadoutSO.SetSlotValue(i, -1);
                }
                else
                {
                    _loadoutSO.SetSlotValue(i, _selectedItems[i].Index);
                }
            }
        }
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
