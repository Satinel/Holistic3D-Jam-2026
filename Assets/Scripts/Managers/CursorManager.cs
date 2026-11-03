using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    bool _optionsEnabled, _inventoryEnabled;

    void Awake()
    {
        HideCursor();
    }

    void OnEnable()
    {
        OptionsMenu.OnOptionsCanvasToggled += OptionsMenu_OnOptionsCanvasToggled;
        Inventory.OnInventoryToggled += Inventory_OnInventoryToggled;
    }

    void OnDisable()
    {
        OptionsMenu.OnOptionsCanvasToggled -= OptionsMenu_OnOptionsCanvasToggled;
        Inventory.OnInventoryToggled -= Inventory_OnInventoryToggled;
    }

    void OptionsMenu_OnOptionsCanvasToggled(bool optionsEnabled)
    {
        _optionsEnabled = optionsEnabled;
        SetShouldShowCursor(_optionsEnabled || _inventoryEnabled);
    }

    void Inventory_OnInventoryToggled(bool inventoryEnabled)
    {
        _inventoryEnabled = inventoryEnabled;
        SetShouldShowCursor(_optionsEnabled || _inventoryEnabled);
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetShouldShowCursor(bool shouldShow)
    {
        if(shouldShow)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }
}
