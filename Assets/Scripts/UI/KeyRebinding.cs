using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebinding : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _actionNameText, _rebindButtonText, _defautButtonText;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    void Awake()
    {
        InputManager.TestSendingInputActions += InputManager_TestSendingInputActions;
    }

    void OnDestroy()
    {
        InputManager.TestSendingInputActions -= InputManager_TestSendingInputActions;
        _rebindOperation?.Cancel();
        _rebindOperation?.Dispose();
        _rebindOperation = null;
    }

    void InputManager_TestSendingInputActions(InputAction inputAction)
    {
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
        _actionNameText.text = inputAction.bindings[0].name;
        _rebindButtonText.text = inputAction.GetBindingDisplayString(0, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);//, InputBinding.DisplayStringOptions.DontOmitDevice);
        _defautButtonText.text = $"Restore Default\n[{InputControlPath.ToHumanReadableString(inputAction.bindings[0].path, InputControlPath.HumanReadableStringOptions.OmitDevice)}]";
    }

    void KeyboardButtonRebindAction(InputAction inputAction, int bindingIndex)
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

    void GamepadButtonRebindAction(InputAction inputAction, int bindingIndex)
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
