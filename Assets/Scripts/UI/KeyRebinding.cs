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
    }

    void OnDestroy()
    {
        InputManager.ReportMoveAction -= InputManager_ReportMoveAction;
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
        // for(int i = 0; i < inputAction.bindings.Count; i++)
        // {
            // if(inputAction.bindings[i].isComposite)
            // {
            //     Debug.Log(i + " is composite");
            //     continue;
            // }
            // Debug.Log(inputAction.bindings[i].path);
            // Debug.Log(inputAction.GetBindingDisplayString(i));
        // }

        // inputAction.ApplyBindingOverride(0, "<Keyboard>/space"); // This works! (But there's no reason to use it, this is just a test)

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
