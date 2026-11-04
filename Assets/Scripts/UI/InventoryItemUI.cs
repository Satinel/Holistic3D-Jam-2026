using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<InventoryItemUI> OnAnyItemButtonPressed;
    public static event Action<InventoryItemUI> OnAnyItemButtonSelected;
    public static event Action<Item, bool> OnAnyItemButtonPointedTo;

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

    public void OnSelect(BaseEventData _)  // EventSystems ISelectHandler Interface
    {
        OnAnyItemButtonSelected?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData _)
    {
        OnAnyItemButtonPointedTo?.Invoke(_item, true);
    }

    public void OnPointerExit(PointerEventData _)
    {
        OnAnyItemButtonPointedTo?.Invoke(_item, false);
    }
    // TODO : Update appearance (highlight, locked/checked icon) based on if in hotbar and if .Item.WasUsed
}
