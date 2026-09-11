using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebinding : MonoBehaviour
{
    [SerializeField] RebindActionUI _rebindActionUIPrefab;
    [SerializeField] Transform _keyboardBindingsParent, _gamepadBindingsParent;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    void Awake()
    {
        InputManager.ReportMoveAction += InputManager_ReportMoveAction;
        InputManager.ReportRebindableActions += InputManager_ReportRebindableActions;
    }

    void OnDestroy()
    {
        InputManager.ReportMoveAction -= InputManager_ReportMoveAction;
        InputManager.ReportRebindableActions -= InputManager_ReportRebindableActions;
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
        }

        // inputAction.ApplyBindingOverride(0, "<Keyboard>/space"); // This works! (But there's no reason to use it, this is just a test)
    }

    void InputManager_ReportRebindableActions(InputAction[] rebindableActions)
    {
        for(int i = 0; i < rebindableActions.Length; i++)
        {
            RebindActionUI keyboardRebindUI = Instantiate(_rebindActionUIPrefab, _keyboardBindingsParent);
            keyboardRebindUI.Initialize(this, rebindableActions[i], 0, false);

            RebindActionUI gamepadRebindUI = Instantiate(_rebindActionUIPrefab, _gamepadBindingsParent);
            gamepadRebindUI.Initialize(this, rebindableActions[i], 1, true);
        }
    }

    public void KeyboardButtonRebindAction(InputAction inputAction, int bindingIndex)
    {
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        inputAction.Disable();

        _rebindOperation = inputAction.PerformInteractiveRebinding()
                                .WithExpectedControlType("Button")
                                .WithCancelingThrough("<Keyboard>/escape")
                                .WithControlsExcluding("<Gamepad>")
                                .WithTargetBinding(bindingIndex)
                                .OnMatchWaitForAnother(0.1f)
                                .OnComplete(operation =>
                                {
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                })
                                .OnCancel(operation =>
                                {
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

        _rebindOperation = inputAction.PerformInteractiveRebinding()
                                .WithExpectedControlType("Button")
                                .WithCancelingThrough("<Gamepad>/start")
                                .WithControlsExcluding("<Keyboard>")
                                .WithControlsExcluding("<Mouse>")
                                .WithTargetBinding(bindingIndex)
                                .OnMatchWaitForAnother(0.1f)
                                .OnComplete(operation =>
                                {
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                })
                                .OnCancel(operation =>
                                {
                                    inputAction.Enable();
                                    operation.Dispose();
                                    _rebindOperation = null;
                                });

        _rebindOperation.Start();
    }

    public void ResetBinding(InputAction inputAction, int index)
    {
        inputAction.RemoveBindingOverride(index);
    }

    public void ToggleDisplayedBindings()
    {
        if(_keyboardBindingsParent.gameObject.activeSelf)
        {
            _keyboardBindingsParent.gameObject.SetActive(false);
            _gamepadBindingsParent.gameObject.SetActive(true);
        }
        else
        {
            _gamepadBindingsParent.gameObject.SetActive(false);
            _keyboardBindingsParent.gameObject.SetActive(true);
        }
    }

    // TODO : Save rebind overrides via json in playerprefs
    // TODO : Load rebind overrides via json in playerprefs


//----------------------------------------- https://www.youtube.com/watch?v=y6oXjn0PSDs ---------------------------------------------------------
    InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    void OnRemapButtonPressed(InputAction action)
    {
        action.Disable();

        _rebindingOperation = action.PerformInteractiveRebinding()
                                .WithControlsExcluding("Mouse")
                                .WithTargetBinding(0)
                                .OnMatchWaitForAnother(0.1f);
        _rebindingOperation.Start();
        _rebindingOperation.OnComplete(OnRebindOperationComplete);
    }

    void OnRebindOperationComplete(InputActionRebindingExtensions.RebindingOperation operation)
    {
        operation.action.Enable();
        _rebindingOperation.Cancel();
    }
}
