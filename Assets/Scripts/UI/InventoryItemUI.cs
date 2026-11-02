using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public static event Action<InventoryItemUI> OnAnyItemButtonPressed;

    [SerializeField] Image _icon, _highlight;
    [SerializeField] TextMeshProUGUI _nameText, _costText;
    [SerializeField] Color _trapColor, _baseColor;

    Item _item;

    public Item Item => _item;

    public void ToggleItem()
    {
        OnAnyItemButtonPressed?.Invoke(this);
    }

    public void Initialize(Item item, int index)
    {
        _item = item;
        _item.SetIndex(index);
        _icon.sprite = _item.Icon;
        _nameText.text = _item.ItemName;
        _costText.text = _item.Cost <= 0 ? string.Empty : _item.Cost.ToString();
        _costText.color = _item.IsTrap ? _trapColor : _baseColor;
    }

    // TODO : Update appearance (highlight, locked/checked icon) based on if in hotbar and if .Item.WasUsed
}
