using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DisplayKeybinds : MonoBehaviour
{
    public static event Action<bool> OnInputDeviceChanged;

    [SerializeField] GameObject[] _keyboardControls, _gamepadControls;

    bool _isShowingKeyboard;

    void Start()
    {
        ToggleActiveBinds(true);
    }

    void Update()
    {
        if(!_isShowingKeyboard)
        {
            if(Keyboard.current != null)
            {
                if(Keyboard.current.anyKey.isPressed)
                {
                    ToggleActiveBinds(true);
                    return;
                }
            }
            if(Mouse.current != null)
            {
                foreach(var control in Mouse.current.allControls)
                {
                    if(control is ButtonControl button && button.isPressed)
                    {
                        ToggleActiveBinds(true);
                        return;
                    }
                }
            }
        }
        else if(Gamepad.current != null)
        {
            foreach(var control in Gamepad.current.allControls)
            {
                if(control is ButtonControl button && button.isPressed)
                {
                    ToggleActiveBinds(false);
                    return;
                }
                if(control is StickControl stickControl && stickControl.ReadValue() != Vector2.zero)
                {
                    ToggleActiveBinds(false);
                    return;
                }
            }
        }
    }

    void ToggleActiveBinds(bool showKeyboard)
    {
        _isShowingKeyboard = showKeyboard;

        foreach(GameObject keyControls in _keyboardControls)
        {
            keyControls.SetActive(showKeyboard);
        }

        foreach(GameObject padControls in _gamepadControls)
        {
            padControls.SetActive(!showKeyboard);
        }

        OnInputDeviceChanged?.Invoke(showKeyboard);
    }
}
