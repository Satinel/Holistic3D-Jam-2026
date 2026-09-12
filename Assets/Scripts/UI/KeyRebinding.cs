using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class KeyRebinding : MonoBehaviour
{
    [SerializeField] InputActionAsset _inputActions;
    [SerializeField] Canvas _rebindCanvas;
    [SerializeField] RebindActionUI _rebindActionUIPrefab;
    [SerializeField] GameObject _keyboardUI, _gamepadUI, _keyboadMainMenuButton, _gamepadMainMenuButton;
    [SerializeField] Transform _keyboardBindingsParent, _gamepadBindingsParent;
    [SerializeField] GameObject _keyboardPrompt, _gamepadPrompt;

    readonly List<RebindActionUI> _instantiatedKeyboardRebinds = new(), _instantiatedGamepadRebinds = new();
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    static readonly string OVERRIDES_STRING = "BindingOverrides";

    void Awake()
    {
        LoadOverrides();
        InputManager.ReportMoveAction += InputManager_ReportMoveAction;
        InputManager.ReportRebindableActions += InputManager_ReportRebindableActions;
        InputManager.OnOptionsPressed += InputManager_OnOptionsPressed;
    }

    void OnDestroy()
    {
        InputManager.ReportMoveAction -= InputManager_ReportMoveAction;
        InputManager.ReportRebindableActions -= InputManager_ReportRebindableActions;
        InputManager.OnOptionsPressed -= InputManager_OnOptionsPressed;
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        _rebindOperation = null;
    }

    void InputManager_ReportMoveAction(InputAction moveAction)
    {
        for(int i = 1; i < 5; i++)
        {
            RebindActionUI rebindUI = Instantiate(_rebindActionUIPrefab, _keyboardBindingsParent);
            rebindUI.Initialize(this, moveAction, i, false);
            _instantiatedKeyboardRebinds.Add(rebindUI);
        }
    }

    void InputManager_ReportRebindableActions(InputAction[] rebindableActions)
    {
        for(int i = 0; i < rebindableActions.Length; i++)
        {
            RebindActionUI keyboardRebindUI = Instantiate(_rebindActionUIPrefab, _keyboardBindingsParent);
            keyboardRebindUI.Initialize(this, rebindableActions[i], 0, false);
            _instantiatedKeyboardRebinds.Add(keyboardRebindUI);
            // TODO : Set navigation of buttons

            RebindActionUI gamepadRebindUI = Instantiate(_rebindActionUIPrefab, _gamepadBindingsParent);
            gamepadRebindUI.Initialize(this, rebindableActions[i], 1, true);
            _instantiatedGamepadRebinds.Add(gamepadRebindUI);
            // TODO : Set navigation of buttons
        }
    }

    void InputManager_OnOptionsPressed()
    {
        _rebindOperation?.Cancel();
    }

    public void KeyboardButtonRebindAction(InputAction inputAction, int bindingIndex)
    {
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        inputAction.Disable();

        _keyboardPrompt.SetActive(true);

        _rebindOperation = inputAction.PerformInteractiveRebinding()
                                .WithExpectedControlType("Button")
                                .WithCancelingThrough("<Keyboard>/escape")
                                .WithControlsExcluding("<Gamepad>")
                                .WithTargetBinding(bindingIndex)
                                .OnMatchWaitForAnother(0.1f)
                                .OnComplete(operation =>
                                {
                                    _keyboardPrompt.SetActive(false);
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                    SaveOverrides();
                                })
                                .OnCancel(operation =>
                                {
                                    _keyboardPrompt.SetActive(false);
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                });

        _rebindOperation.Start();
    }

    public void GamepadButtonRebindAction(InputAction inputAction, int bindingIndex)
    {
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        inputAction.Disable();

        _gamepadPrompt.SetActive(true);

        _rebindOperation = inputAction.PerformInteractiveRebinding()
                                .WithExpectedControlType("Button")
                                .WithCancelingThrough("<Gamepad>/start")
                                .WithControlsExcluding("<Keyboard>")
                                .WithControlsExcluding("<Mouse>")
                                .WithTargetBinding(bindingIndex)
                                .OnMatchWaitForAnother(0.1f)
                                .OnComplete(operation =>
                                {
                                    _gamepadPrompt.SetActive(false);
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                    SaveOverrides();
                                })
                                .OnCancel(operation =>
                                {
                                    _gamepadPrompt.SetActive(false);
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                });

        _rebindOperation.Start();
    }

    public void ResetBinding(InputAction inputAction, int index)
    {
        inputAction.RemoveBindingOverride(index);
        SaveOverrides();
    }

    public void ResetAllBindings()
    {
        ResetAllKeyboardBindings();
        ResetAllGamepadBindings();
        SaveOverrides();
    }

    public void ResetAllKeyboardBindings()
    {
        foreach(RebindActionUI rebind in _instantiatedKeyboardRebinds)
        {
            rebind.DefaultButton();
        }
    }

    public void ResetAllGamepadBindings()
    {
        foreach(RebindActionUI rebind in _instantiatedGamepadRebinds)
        {
            rebind.DefaultButton();
        }
    }

    public void ToggleDisplayedBindings()
    {
        if(_keyboardUI.activeSelf)
        {
            _keyboardUI.SetActive(false);
            _gamepadUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_gamepadMainMenuButton);
        }
        else
        {
            _gamepadUI.SetActive(false);
            _keyboardUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_keyboadMainMenuButton);
        }
    }

    public void EnableUI()
    {
        _rebindCanvas.enabled = true;
        if(_keyboardUI.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_keyboadMainMenuButton);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_gamepadMainMenuButton);
        }
    }

    public void DisableUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _rebindCanvas.enabled = false;
    }

    void SaveOverrides()
    {
        PlayerPrefs.SetString(OVERRIDES_STRING, _inputActions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    void LoadOverrides()
    {
        string savedOverrides = PlayerPrefs.GetString(OVERRIDES_STRING, "");

        if(!string.IsNullOrEmpty(savedOverrides))
        {
            _inputActions.LoadBindingOverridesFromJson(savedOverrides);
        }
    }


//----------------------------------------- https://www.youtube.com/watch?v=y6oXjn0PSDs ---------------------------------------------------------
    // InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    // void OnRemapButtonPressed(InputAction action)
    // {
    //     action.Disable();

    //     _rebindingOperation = action.PerformInteractiveRebinding()
    //                             .WithControlsExcluding("Mouse")
    //                             .WithTargetBinding(0)
    //                             .OnMatchWaitForAnother(0.1f);
    //     _rebindingOperation.Start();
    //     _rebindingOperation.OnComplete(OnRebindOperationComplete);
    // }

    // void OnRebindOperationComplete(InputActionRebindingExtensions.RebindingOperation operation)
    // {
    //     operation.action.Enable();
    //     _rebindingOperation.Cancel();
    // }
}
