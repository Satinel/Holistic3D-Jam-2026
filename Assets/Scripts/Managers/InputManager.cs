using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2> OnMoveAction;
    public static event Action<Vector2> OnLookAction;
    public static event Action OnMainPressed, OnSecondaryPressed;
    public static event Action OnSellPressed, OnOptionsPressed;
    public static event Action<Vector2> OnScroll;
    public static event Action OnPreviousPressed, OnNextPressed;

    InputAction _moveAction, _lookAction;
    InputAction _mainAction, _secondaryAction, _sellAction, _optionsAction;
    InputAction _scrollAction, _previousAction, _nextAction;

    void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");
        _mainAction = InputSystem.actions.FindAction("Action");
        _secondaryAction = InputSystem.actions.FindAction("Secondary");
        _sellAction = InputSystem.actions.FindAction("Sell");
        _optionsAction = InputSystem.actions.FindAction("Options");
        _scrollAction = InputSystem.actions.FindAction("Scroll");
        _previousAction = InputSystem.actions.FindAction("Previous");
        _nextAction = InputSystem.actions.FindAction("Next");

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

        if(_sellAction.WasPerformedThisFrame())
        {
            OnSellPressed?.Invoke();
        }

        if(_optionsAction.WasPerformedThisFrame())
        {
            OnOptionsPressed?.Invoke();
        }

        OnScroll?.Invoke(_scrollAction.ReadValue<Vector2>());

        if(_previousAction.WasPerformedThisFrame())
        {
            OnPreviousPressed?.Invoke();
        }

        if(_nextAction.WasPerformedThisFrame())
        {
            OnNextPressed?.Invoke();
        }
    }
}
