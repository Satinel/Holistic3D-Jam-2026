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

    public static event Action<int> On1Pressed, On2Pressed, On3Pressed, On4Pressed, On5Pressed;
    public static event Action<int> On6Pressed, On7Pressed, On8Pressed, On9Pressed, On10Pressed;

    InputAction _moveAction, _lookAction;
    InputAction _mainAction, _secondaryAction, _sellAction, _optionsAction;
    InputAction _scrollAction, _previousAction, _nextAction;

    InputAction _1Action, _2Action, _3Action, _4Action, _5Action, _6Action, _7Action, _8Action, _9Action, _10Action;

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

        _1Action = InputSystem.actions.FindAction("1");
        _2Action = InputSystem.actions.FindAction("2");
        _3Action = InputSystem.actions.FindAction("3");
        _4Action = InputSystem.actions.FindAction("4");
        _5Action = InputSystem.actions.FindAction("5");
        _6Action = InputSystem.actions.FindAction("6");
        _7Action = InputSystem.actions.FindAction("7");
        _8Action = InputSystem.actions.FindAction("8");
        _9Action = InputSystem.actions.FindAction("9");
        _10Action = InputSystem.actions.FindAction("10");
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

        if(_1Action.WasPerformedThisFrame())
        {
            On1Pressed?.Invoke(0);
        }
        if(_2Action.WasPerformedThisFrame())
        {
            On2Pressed?.Invoke(1);
        }
        if(_3Action.WasPerformedThisFrame())
        {
            On3Pressed?.Invoke(2);
        }
        if(_4Action.WasPerformedThisFrame())
        {
            On4Pressed?.Invoke(3);
        }
        if(_5Action.WasPerformedThisFrame())
        {
            On5Pressed?.Invoke(4);
        }
        if(_6Action.WasPerformedThisFrame())
        {
            On6Pressed?.Invoke(5);
        }
        if(_7Action.WasPerformedThisFrame())
        {
            On7Pressed?.Invoke(6);
        }
        if(_8Action.WasPerformedThisFrame())
        {
            On8Pressed?.Invoke(7);
        }
        if(_9Action.WasPerformedThisFrame())
        {
            On9Pressed?.Invoke(8);
        }
        if(_10Action.WasPerformedThisFrame())
        {
            On10Pressed?.Invoke(9);
        }

    }
}
