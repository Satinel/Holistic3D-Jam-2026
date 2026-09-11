using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindActionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _actionNameText, _rebindButtonText, _defautButtonText;
    [SerializeField] int _index;
    [SerializeField] bool _isGamepadBinding;
    KeyRebinding _keyRebinding;
    InputAction _inputAction;

    public void Initialize(KeyRebinding keyRebinding, InputAction inputAction, int index, bool isGamepadBinding)
    {
        _keyRebinding = keyRebinding;
        _inputAction = inputAction;
        _index = index;
        _isGamepadBinding = isGamepadBinding;

        _actionNameText.text = string.IsNullOrWhiteSpace(_inputAction.bindings[index].name) ? _inputAction.name : _inputAction.bindings[_index].name;
        _rebindButtonText.text = _inputAction.GetBindingDisplayString(_index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
        _defautButtonText.text = $"Restore Default\n[{InputControlPath.ToHumanReadableString(_inputAction.bindings[_index].path, InputControlPath.HumanReadableStringOptions.OmitDevice)}]";
    }

    public void RebindButton()
    {
        if(_isGamepadBinding)
        {
            _keyRebinding.GamepadButtonRebindAction(_inputAction, _index);
        }
        else
        {
            _keyRebinding.KeyboardButtonRebindAction(_inputAction, _index);
        }
    }

    public void DefaultButton()
    {
        _keyRebinding.ResetBinding(_inputAction, _index);
        _rebindButtonText.text = _inputAction.GetBindingDisplayString(_index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
    }
}
