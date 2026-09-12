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
    bool _isDestroyed;

    void OnDestroy()
    {
        _isDestroyed = true;    // Alledgedly this can be read after being set true
    }

    public void Initialize(KeyRebinding keyRebinding, InputAction inputAction, int index, bool isGamepadBinding)
    {
        _keyRebinding = keyRebinding;
        _inputAction = inputAction;
        _index = index;
        _isGamepadBinding = isGamepadBinding;

        _actionNameText.text = string.IsNullOrWhiteSpace(_inputAction.bindings[index].name) ? _inputAction.name : _inputAction.bindings[_index].name;
        _defautButtonText.text = $"Restore Default\n[{InputControlPath.ToHumanReadableString(_inputAction.bindings[_index].path, InputControlPath.HumanReadableStringOptions.OmitDevice)}]";
        SetRebindButtonText();
    }

    void SetRebindButtonText()
    {
        if(_isDestroyed) { return; }    // Probably totally unnecessary safeguard that would never come up (and does it really work?)

        _rebindButtonText.text = _inputAction.GetBindingDisplayString(_index, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
    }

    public void RebindButton()
    {
        if(_isGamepadBinding)
        {
            _keyRebinding.GamepadButtonRebindAction(_inputAction, _index, SetRebindButtonText);
        }
        else
        {
            _keyRebinding.KeyboardButtonRebindAction(_inputAction, _index, SetRebindButtonText);
        }
    }

    public void DefaultButton()
    {
        _keyRebinding.ResetBinding(_inputAction, _index);
        SetRebindButtonText();
    }
}
