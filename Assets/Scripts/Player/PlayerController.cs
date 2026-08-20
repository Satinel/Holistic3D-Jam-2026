using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 2.5f, _rotateSpeed = 1.5f;
    [SerializeField] float _minLookAngle = -25f, _maxLookAngle = 40f;
    [SerializeField] float _modelRotateSpeed = 15f;
    [SerializeField] CharacterController _characterController;
    [SerializeField] Transform _cameraTarget, _model, _aimPositionMarker;
    [SerializeField] CinemachineThirdPersonAim _cinemachineThirdPersonAim;
    [SerializeField] Health _myHealth;
    [SerializeField] Mana _myMana;
    [SerializeField] Animator _animator;

    Vector2 _moveInputValue = Vector2.zero, _lookAccumulation = Vector2.zero;
    float _currentXAngle = 0f;
    bool _isDead;

    static readonly int DEATH_HASH = Animator.StringToHash("Death");

    void Awake()
    {
        _myHealth.OnDeath += MyHealth_OnDeath;
    }

    void OnDestroy()
    {
        _myHealth.OnDeath -= MyHealth_OnDeath;
    }

    void OnEnable()
    {
        InputManager.OnMoveAction += InputManager_OnMoveAction;
        InputManager.OnLookAction += InputManager_OnLookAction;
        InputManager.OnMainPressed += InputManager_OnMainPressed;
        InputManager.OnSecondaryPressed += InputManager_OnSecondaryPressed;
    }

    void OnDisable()
    {
        InputManager.OnMoveAction -= InputManager_OnMoveAction;
        InputManager.OnLookAction -= InputManager_OnLookAction;
        InputManager.OnMainPressed -= InputManager_OnMainPressed;
        InputManager.OnSecondaryPressed -= InputManager_OnSecondaryPressed;
    }

    void Update()
    {
        MovePlayer();
    }

    void LateUpdate()
    {
        RotateCameraTarget();
        _aimPositionMarker.position = _cinemachineThirdPersonAim.AimTarget;
    }

    void MovePlayer()
    {
        if(_moveInputValue.magnitude > 0)
        {
            Vector3 right = _cameraTarget.right;
            Vector3 forward = _cameraTarget.forward;

            right.y = 0;
            forward.y = 0;

            Vector3 direction = (_moveInputValue.x * right) + (_moveInputValue.y * forward);
            direction = new Vector3(direction.x, 0f, direction.z).normalized;

            _characterController.Move(_moveSpeed * Time.deltaTime * direction);

            RotateModel();
        }
    }

    void RotateModel()
    {
        if(_isDead) { return; }

        Vector3 cameraForward = _cameraTarget.forward;
        cameraForward.y = 0;
        _model.rotation = Quaternion.Slerp(_model.rotation, Quaternion.LookRotation(cameraForward), _modelRotateSpeed * Time.deltaTime);
    }

    void RotateModelInstantly()
    {
        Vector3 cameraForward = _cameraTarget.forward;
        cameraForward.y = 0;
        _model.rotation = Quaternion.LookRotation(cameraForward);
    }

    void RotateCameraTarget()
    {
        float currentYRotation = _cameraTarget.localEulerAngles.y;

        _currentXAngle += _lookAccumulation.y * _rotateSpeed;
        _currentXAngle = Mathf.Clamp(_currentXAngle, _minLookAngle, _maxLookAngle);

        float newYRotation = currentYRotation + (_lookAccumulation.x * _rotateSpeed);

        _cameraTarget.localEulerAngles = new Vector3(_currentXAngle, newYRotation, 0f);

        _lookAccumulation = Vector2.zero;
    }

    void InputManager_OnMoveAction(Vector2 value)
    {
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        _moveInputValue = value;
    }

    void InputManager_OnLookAction(Vector2 value)
    {
        if(Time.timeScale == 0) { return; }

        _lookAccumulation += value;
    }
[SerializeField] Rigidbody _cannonballPrefab;   // TODO : Remove, this is for testing
Vector3 _spawnPosition = new(0f, 1f, 0f);
    void InputManager_OnMainPressed()
    {
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        RotateModelInstantly();
        // Do an action like attack/place tower/trap/thing
        Rigidbody cannonball = Instantiate(_cannonballPrefab, transform.position + _spawnPosition, Quaternion.identity);
        cannonball.transform.forward = _aimPositionMarker.position - cannonball.transform.position;
        cannonball.AddForce(cannonball.transform.forward * 25f, ForceMode.Impulse);
    }

    void InputManager_OnSecondaryPressed()
    {
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        RotateModelInstantly();
        // Do a secondary action like cast a knockback spell or something
    }

    void MyHealth_OnDeath()
    {
        if(!_isDead)
        {
            _isDead = true;
            _moveInputValue = Vector2.zero;
            if(_animator)
            {
                _animator.SetTrigger(DEATH_HASH);
                // TODO : Attach an animator and have a death animation (and a model, etc.)
            }
        }
    }
}
