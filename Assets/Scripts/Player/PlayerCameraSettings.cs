using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraSettings : MonoBehaviour
{
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] CinemachineThirdPersonFollow _thirdPersonCamera;
    [SerializeField] CameraOptionsSO _cameraOptions;
    [SerializeField] float _cameraSideCenter = 0.5f;
    [SerializeField] Vector3 _firstPersonCameraOffset = new(0f, -1f, 1.75f);
    [SerializeField] Vector3 _thirdPersonCameraOffset = new(0.8f, -1f, 0f);
    [SerializeField] GameObject _playerModel;

    bool _isFirstPerson;    // This exists so the player can toggle between first and third person without going into options or saving/loading from PlayerPrefs

    void Awake()
    {
        OptionsMenu.OnCameraValuesChanged += OptionsMenu_OnCameraValuesChanged;
        InputManager.OnViewChangePressed += InputManager_OnViewChangePressed;
    }

    void OnDestroy()
    {
        OptionsMenu.OnCameraValuesChanged -= OptionsMenu_OnCameraValuesChanged;
        InputManager.OnViewChangePressed -= InputManager_OnViewChangePressed;
    }

    void OptionsMenu_OnCameraValuesChanged()
    {
        _camera.Lens.FieldOfView = _cameraOptions.FieldOfView;

        if(_cameraOptions.IsFirstPerson)
        {
            EnterFirstPerson();
        }
        else
        {
            ExitFirstPerson();
        }
    }

    void InputManager_OnViewChangePressed()
    {
        ToggleFirstPerson();
    }

    void EnterFirstPerson()
    {
        _thirdPersonCamera.ShoulderOffset = _firstPersonCameraOffset;
        _thirdPersonCamera.CameraSide = _cameraSideCenter;
        _playerModel.SetActive(false);
        _isFirstPerson = true;
    }

    void ExitFirstPerson()
    {
        _thirdPersonCamera.ShoulderOffset = _thirdPersonCameraOffset;
        _thirdPersonCamera.CameraSide = _cameraOptions.CameraSide;
        _playerModel.SetActive(true);
        _isFirstPerson = false;
    }

    public void ToggleFirstPerson()
    {
        if(_isFirstPerson)
        {
            ExitFirstPerson();
        }
        else
        {
            EnterFirstPerson();
        }
    }
}
