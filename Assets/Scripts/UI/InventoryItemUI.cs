using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public static event Action<Item, int> OnAnyItemButtonPressed;

    [SerializeField] Image _icon, _highlight;
    [SerializeField] TextMeshProUGUI _nameText, _costText;
    [SerializeField] Color _trapColor, _baseColor;

    Item _item;
    int _index;

    public void ToggleItem()
    {
        OnAnyItemButtonPressed?.Invoke(_item, _index);  // TODO : Make this add/remove _item from Inventory's _selectedItems as appropriate (unless _item has been used)
    }

    internal void Initialize(Item item, int index)
    {
        _item = item;
        _index = index;
        _icon.sprite = _item.Icon;
        _nameText.text = _item.ItemName;
        _costText.text = _item.Cost <= 0 ? string.Empty : _item.Cost.ToString();
        _costText.color = _item.IsTrap ? _trapColor : _baseColor;
    }
}
