using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyRebinding : MonoBehaviour
{
    [SerializeField] InputActionAsset _inputActions;
    [SerializeField] Canvas _rebindCanvas;
    [SerializeField] RebindActionUI _rebindActionUIPrefab;
    [SerializeField] GameObject _keyboardUI, _gamepadUI;
    [SerializeField] Button _keyboadMainMenuButton, _gamepadMainMenuButton;
    [SerializeField] Transform _keyboardBindingsParent, _gamepadBindingsParent;
    [SerializeField] GameObject _keyboardPrompt, _gamepadPrompt;

    readonly List<RebindActionUI> _instantiatedKeyboardRebinds = new(), _instantiatedGamepadRebinds = new();
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    InputAction _navigateAction;

    static readonly string OVERRIDES_STRING = "BindingOverrides";

    void Awake()
    {
        _navigateAction = _inputActions.FindAction("UI/Navigate");

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
            rebindUI.Initialize(this, moveAction, i, false, true);
            _instantiatedKeyboardRebinds.Add(rebindUI);
        }
    }

    void InputManager_ReportRebindableActions(InputAction[] rebindableActions)
    {
        for (int i = 0; i < rebindableActions.Length; i++)
        {
            RebindActionUI keyboardRebindUI = Instantiate(_rebindActionUIPrefab, _keyboardBindingsParent);
            keyboardRebindUI.Initialize(this, rebindableActions[i], 0, false);
            _instantiatedKeyboardRebinds.Add(keyboardRebindUI);

            RebindActionUI gamepadRebindUI = Instantiate(_rebindActionUIPrefab, _gamepadBindingsParent);
            gamepadRebindUI.Initialize(this, rebindableActions[i], 1, true);
            _instantiatedGamepadRebinds.Add(gamepadRebindUI);
        }

        SetNavigations(_instantiatedKeyboardRebinds, _keyboadMainMenuButton);
        SetNavigations(_instantiatedGamepadRebinds, _gamepadMainMenuButton);
    }

    void SetNavigations(List<RebindActionUI> actionUIList, Button mainMenuButton)
    {
        for(int i = 0; i < actionUIList.Count; i++)
        {
            if(i == 0)
            {
                actionUIList[i].SetFirstButtonNavigation(mainMenuButton, actionUIList[i + 1]);
            }
            else if(i < actionUIList.Count - 1)
            {
                actionUIList[i].SetButtonNavigation(actionUIList[i - 1], actionUIList[i + 1]);
            }
            else
            {
                actionUIList[i].SetLastButtonNavigation(actionUIList[i - 1], mainMenuButton);
            }
        }

        Navigation navigation = mainMenuButton.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnDown = actionUIList[0].RebindB;
        navigation.selectOnUp = actionUIList[^1].RebindB;
        mainMenuButton.navigation = navigation;
    }

    void InputManager_OnOptionsPressed()
    {
        if(Time.timeScale > 0) { return; }

        _rebindOperation?.Cancel();
    }

    public void KeyboardButtonRebindAction(InputAction inputAction, int bindingIndex, Action onComplete)
    {
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        inputAction.Disable();

        _keyboardPrompt.SetActive(true);

        _rebindOperation = inputAction.PerformInteractiveRebinding()
                                .WithExpectedControlType("Button")
                                .WithCancelingThrough("<Keyboard>/escape")
                                .WithCancelingThrough("<Keyboard>/tab")
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
                                    onComplete?.Invoke();
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

    public void RebindNavigate(int bindingIndex, string bindingPath)
    {
        _navigateAction.ApplyBindingOverride(bindingIndex, bindingPath);
        SaveOverrides();
    }

    public void GamepadButtonRebindAction(InputAction inputAction, int bindingIndex, Action onComplete)
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
                                    onComplete?.Invoke();
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
            EventSystem.current.SetSelectedGameObject(_gamepadMainMenuButton.gameObject);
        }
        else
        {
            _gamepadUI.SetActive(false);
            _keyboardUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_keyboadMainMenuButton.gameObject);
        }
    }

    public void EnableUI()
    {
        _rebindCanvas.enabled = true;
        if(_keyboardUI.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_keyboadMainMenuButton.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_gamepadMainMenuButton.gameObject);
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
