using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2> OnMoveAction;
    public static event Action<Vector2> OnLookAction;
    public static event Action OnMainPressed, OnSecondaryPressed;
    public static event Action OnOptionsPressed;

    InputAction _moveAction, _lookAction;
    InputAction _mainAction, _secondaryAction, _optionsAction;

    void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");
        _mainAction = InputSystem.actions.FindAction("Action");
        _secondaryAction = InputSystem.actions.FindAction("Secondary");
        _optionsAction = InputSystem.actions.FindAction("Options");
    }

    void Update()
    {
        OnMoveAction?.Invoke(_moveAction.ReadValue<Vector2>());
        OnLookAction?.Invoke(_lookAction.ReadValue<Vector2>());

        if(_mainAction.WasPerformedThisFrame())
        {
            OnMainPressed?.Invoke();
        }

        if(_secondaryAction.WasPerformedThisFrame())
        {
            OnSecondaryPressed?.Invoke();
        }

        if(_optionsAction.WasPerformedThisFrame())
        {
            OnOptionsPressed?.Invoke();
        }
    }
}
