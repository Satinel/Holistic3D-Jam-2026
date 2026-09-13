using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RebindActionUI : MonoBehaviour
{
    [field:SerializeField] public Button RebindB { get; private set; }
    [field:SerializeField] public Button DefaultB { get; private set; }
    [SerializeField] TextMeshProUGUI _actionNameText, _rebindButtonText, _defautButtonText;
    int _index;
    bool _isGamepadBinding, _isMoveBinding;
    KeyRebinding _keyRebinding;
    InputAction _inputAction;
    bool _isDestroyed;

    void OnDestroy()
    {
        _isDestroyed = true;    // Alledgedly this can be read after being set true
    }

    public void Initialize(KeyRebinding keyRebinding, InputAction inputAction, int index, bool isGamepadBinding, bool isMoveBinding = false)
    {
        _keyRebinding = keyRebinding;
        _inputAction = inputAction;
        _index = index;
        _isGamepadBinding = isGamepadBinding;
        _isMoveBinding = isMoveBinding;

        _actionNameText.text = string.IsNullOrWhiteSpace(_inputAction.bindings[index].name) ? _inputAction.name : _inputAction.bindings[_index].name;
        _rebindButtonText.text = _inputAction.GetBindingDisplayString(_index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        _defautButtonText.text = $"Restore Default\n[{InputControlPath.ToHumanReadableString(_inputAction.bindings[_index].path, InputControlPath.HumanReadableStringOptions.OmitDevice)}]";
    }

    public void SetFirstButtonNavigation(Button button, RebindActionUI next)
    {
        Navigation rebindNavigation = RebindB.navigation;
        rebindNavigation.mode = Navigation.Mode.Explicit;
        rebindNavigation.selectOnUp = button;
        rebindNavigation.selectOnDown = next.RebindB;
        RebindB.navigation = rebindNavigation;

        Navigation defaultNavigation = DefaultB.navigation;
        defaultNavigation.mode = Navigation.Mode.Explicit;
        defaultNavigation.selectOnUp = button;
        defaultNavigation.selectOnDown = next.DefaultB;
        DefaultB.navigation = defaultNavigation;
    }

    public void SetButtonNavigation(RebindActionUI previous, RebindActionUI next)
    {
        Navigation rebindNavigation = RebindB.navigation;
        rebindNavigation.mode = Navigation.Mode.Explicit;
        rebindNavigation.selectOnUp = previous.RebindB;
        rebindNavigation.selectOnDown = next.RebindB;
        RebindB.navigation = rebindNavigation;

        Navigation defaultNavigation = DefaultB.navigation;
        defaultNavigation.mode = Navigation.Mode.Explicit;
        defaultNavigation.selectOnUp = previous.DefaultB;
        defaultNavigation.selectOnDown = next.DefaultB;
        DefaultB.navigation = defaultNavigation;
    }

    public void SetLastButtonNavigation(RebindActionUI previous, Button button)
    {
        Navigation rebindNavigation = RebindB.navigation;
        rebindNavigation.mode = Navigation.Mode.Explicit;
        rebindNavigation.selectOnUp = previous.RebindB;
        rebindNavigation.selectOnDown = button;
        RebindB.navigation = rebindNavigation;

        Navigation defaultNavigation = DefaultB.navigation;
        defaultNavigation.mode = Navigation.Mode.Explicit;
        defaultNavigation.selectOnUp = previous.DefaultB;
        defaultNavigation.selectOnDown = button;
        DefaultB.navigation = defaultNavigation;
    }

    void RebindCompleteCallback()
    {
        if(_isDestroyed) { return; }    // Probably totally unnecessary safeguard that would never come up (and does it really work?)

        _rebindButtonText.text = _inputAction.GetBindingDisplayString(_index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

        if(_isMoveBinding)
        {
            _keyRebinding.RebindNavigate(_index, _inputAction.bindings[_index].effectivePath);
        }
    }

    public void RebindButton()
    {
        if(_isGamepadBinding)
        {
            _keyRebinding.GamepadButtonRebindAction(_inputAction, _index, RebindCompleteCallback);
        }
        else
        {
            _keyRebinding.KeyboardButtonRebindAction(_inputAction, _index, RebindCompleteCallback);
        }
    }

    public void DefaultButton()
    {
        _keyRebinding.ResetBinding(_inputAction, _index);
        RebindCompleteCallback();
    }
}
