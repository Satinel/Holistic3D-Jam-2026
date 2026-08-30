using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    public event Action<bool> OnCanSellTrap;
    public event Action<Item[]> ReportTotalItems;
    public event Action<int> OnActiveItemChanged;
    public event Action OnTooTired;

    [SerializeField] float _moveSpeed = 2.5f, _backupPenalty = 1f, _rotateSpeed = 1.5f, _sprintSpeed = 2.5f, _respawnDelay = 1.25f;
    [SerializeField] float _minLookAngle = -25f, _maxLookAngle = 40f;
    [SerializeField] float _modelRotateSpeed = 15f;
    [SerializeField] CharacterController _characterController;
    [SerializeField] Transform _cameraTarget, _model, _aimPositionMarker;
    [SerializeField] CinemachineThirdPersonAim _cinemachineThirdPersonAim;
    [SerializeField] Health _myHealth;
    [SerializeField] Mana _myMana;
    [SerializeField] Wallet _wallet;
    [SerializeField] Animator _animator;
    [SerializeField] Item[] _items;
    [SerializeField] LayerMask _socketLayer;
    [SerializeField] Color _buyColor = Color.green, _poorColor = Color.red;
    [SerializeField] GameObject _ballModel;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _attackSFX, _placeTrapSFX, _sellTrapSFX, _hurtSFX;

    Vector3 _respawnPosition = new();
    Vector2 _moveInputValue = Vector2.zero, _lookAccumulation = Vector2.zero;
    float _currentXAngle = 0f;
    bool _isDead, _inSellMode, _canBuyTrap, _canSellTrap, _isLevelOver, _isSprinting, _isAttacking;
    int _itemIndex = 0;
    Item _activeItem;
    BuyableTrap _activeTrap;
    TrapSocket _activeSocket;
    TrapPreview _previewModel;

    static readonly int DEATH_HASH = Animator.StringToHash("Death");
    static readonly int ATTACK_HASH = Animator.StringToHash("Attack");
    static readonly int MOVE_HASH = Animator.StringToHash("IsMoving");
    static readonly int BACKUP_HASH = Animator.StringToHash("IsBackingUp");
    static readonly int SPRINT_HASH = Animator.StringToHash("IsSprinting");
    static readonly int RESPAWN_HASH = Animator.StringToHash("Respawn");

    void Awake()
    {
        _myHealth.OnLoseHealth += _myHealth_OnLoseHealth;
        _myHealth.OnDeath += MyHealth_OnDeath;

        LevelManager.OnWaveStarted += LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted += LevelManager_LevelOver;
        LevelManager.OnLevelFailed += LevelManager_LevelOver;
    }

    void OnDestroy()
    {
        _myHealth.OnLoseHealth += _myHealth_OnLoseHealth;
        _myHealth.OnDeath -= MyHealth_OnDeath;

        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted -= LevelManager_LevelOver;
        LevelManager.OnLevelFailed -= LevelManager_LevelOver;
    }

    void OnEnable()
    {
        InputManager.OnMoveAction += InputManager_OnMoveAction;
        InputManager.OnLookAction += InputManager_OnLookAction;
        InputManager.OnMainPressed += InputManager_OnMainPressed;
        InputManager.OnSecondaryPressed += InputManager_OnSecondaryPressed;
        InputManager.OnSprintHeld += InputManager_OnSprintHeld;
        InputManager.OnSellPressed += InputManager_OnSellPressed;
        InputManager.OnScroll += InputManager_OnScroll;
        InputManager.OnPreviousPressed += InputManager_OnPreviousPressed;
        InputManager.OnNextPressed += InputManager_OnNextPressed;

        InputManager.On1Pressed += SetActiveItemByIndex;
        InputManager.On2Pressed += SetActiveItemByIndex;
        InputManager.On3Pressed += SetActiveItemByIndex;
        InputManager.On4Pressed += SetActiveItemByIndex;
        InputManager.On5Pressed += SetActiveItemByIndex;
        InputManager.On6Pressed += SetActiveItemByIndex;
        InputManager.On7Pressed += SetActiveItemByIndex;
        InputManager.On8Pressed += SetActiveItemByIndex;
        InputManager.On9Pressed += SetActiveItemByIndex;
        InputManager.On10Pressed += SetActiveItemByIndex;
    }

    void OnDisable()
    {
        InputManager.OnMoveAction -= InputManager_OnMoveAction;
        InputManager.OnLookAction -= InputManager_OnLookAction;
        InputManager.OnMainPressed -= InputManager_OnMainPressed;
        InputManager.OnSecondaryPressed -= InputManager_OnSecondaryPressed;
        InputManager.OnSprintHeld -= InputManager_OnSprintHeld;
        InputManager.OnSellPressed -= InputManager_OnSellPressed;
        InputManager.OnScroll -= InputManager_OnScroll;
        InputManager.OnPreviousPressed -= InputManager_OnPreviousPressed;
        InputManager.OnNextPressed -= InputManager_OnNextPressed;

        InputManager.On1Pressed -= SetActiveItemByIndex;
        InputManager.On2Pressed -= SetActiveItemByIndex;
        InputManager.On3Pressed -= SetActiveItemByIndex;
        InputManager.On4Pressed -= SetActiveItemByIndex;
        InputManager.On5Pressed -= SetActiveItemByIndex;
        InputManager.On6Pressed -= SetActiveItemByIndex;
        InputManager.On7Pressed -= SetActiveItemByIndex;
        InputManager.On8Pressed -= SetActiveItemByIndex;
        InputManager.On9Pressed -= SetActiveItemByIndex;
        InputManager.On10Pressed -= SetActiveItemByIndex;
    }

    void Start()
    {
        if(_items.Length > 0)
        {
            ReportTotalItems?.Invoke(_items);
            _activeItem = _items[_itemIndex];
            _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
            OnActiveItemChanged?.Invoke(_itemIndex);
        }

        _inSellMode = true;
        _respawnPosition = transform.position;
    }

    void Update()
    {
        if(_activeItem)
        {
            _ballModel.SetActive(!_activeItem.IsTrap);
        }

        if(_isDead || _isLevelOver) { return; }

        MovePlayer();
        Gravity();
    }

    void LateUpdate()
    {
        if(_isDead || _isLevelOver) { return; }

        RotateCameraTarget();
        _aimPositionMarker.position = _cinemachineThirdPersonAim.AimTarget;
        CheckSocket();
    }

    void MovePlayer()
    {
        if(_moveInputValue.magnitude > 0 && !_isAttacking)
        {
            bool movingBackward = _moveInputValue.y < 0;
            if(movingBackward)
            {
                _animator.SetBool(BACKUP_HASH, true);
                _animator.SetBool(MOVE_HASH, false);
            }
            else if(_moveInputValue.y > 0)
            {
                _animator.SetBool(MOVE_HASH, true);
                _animator.SetBool(BACKUP_HASH, false);
            }
            else
            {
                _animator.SetBool(BACKUP_HASH, true);
                _animator.SetBool(MOVE_HASH, false);
            }

            Vector3 right = _cameraTarget.right;
            Vector3 forward = _cameraTarget.forward;

            right.y = 0;
            forward.y = 0;

            Vector3 direction = (_moveInputValue.x * right) + (_moveInputValue.y * forward);
            direction = new Vector3(direction.x, 0f, direction.z).normalized;

            float penalty = movingBackward ? _backupPenalty : 0;
            float speed = _isSprinting ? _moveSpeed + _sprintSpeed - penalty : _moveSpeed - penalty;
            _characterController.Move(speed * Time.deltaTime * direction);

            RotateModel();
        }
        else
        {
            _animator.SetBool(MOVE_HASH, false);
            _animator.SetBool(BACKUP_HASH, false);
        }
    }

    void Gravity()
    {
        if(transform.position.y > 0)
        {
            transform.position = new(transform.position.x, 0, transform.position.z);
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

    void CheckSocket()
    {
        if(Time.deltaTime == 0) { return; }

        if(!_inSellMode && _activeTrap == null)
        {
            CancelTrapCommerce();
            return;
        }

        _canBuyTrap = false;

        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, _socketLayer))
        {
            if(hit.collider.TryGetComponent(out TrapSocket socket))
            {
                if(_activeSocket && _activeSocket != socket)
                {
                    _activeSocket.HighlightTrap(false);
                    RemoveTrapPreview();
                }
                _activeSocket = socket;

                if(_activeTrap != null && _activeTrap.CanPlaceTrap(_activeSocket))
                {
                    _activeSocket.HighlightTrap(false);
                    _canSellTrap = false;
                    OnCanSellTrap?.Invoke(_canSellTrap);
                    _canBuyTrap = _wallet.CanAfford(_activeTrap.BuyPrice);

                    if(_previewModel == null)
                    {
                        _previewModel = Instantiate(_activeTrap.PreviewPrefab.gameObject).GetComponent<TrapPreview>();
                    }

                    _previewModel.transform.SetPositionAndRotation(socket.transform.position, socket.transform.rotation);

                    if(_canBuyTrap)
                    {
                        _previewModel.SetMaterials(_buyColor);
                    }
                    else
                    {
                        _previewModel.SetMaterials(_poorColor);
                    }
                    _previewModel.ShowRange(_canBuyTrap);
                    return;
                }

                if(_activeSocket.HasTrap)
                {
                    if(!_inSellMode)
                    {
                        CancelTrapCommerce();
                        return;
                    }

                    RemoveTrapPreview();
                    _canBuyTrap = false;
                    _activeSocket.HighlightTrap(true);
                    _canSellTrap = true;
                    OnCanSellTrap?.Invoke(_canSellTrap);
                }
                else
                {
                    _canSellTrap = false;
                    OnCanSellTrap?.Invoke(_canSellTrap);
                }
            }
            else
            {
                CancelTrapCommerce();
            }
        }
        else
        {
            CancelTrapCommerce();
        }
    }

    void CancelTrapCommerce()
    {
        _canBuyTrap = false;
        _canSellTrap = false;
        OnCanSellTrap?.Invoke(_canSellTrap);
        if(_activeSocket != null)
        {
            _activeSocket.HighlightTrap(false);
            _activeSocket = null;
        }
        RemoveTrapPreview();
    }

    void RemoveTrapPreview()
    {
        if(_previewModel != null)
        {
            Destroy(_previewModel.gameObject);
            _previewModel = null;
        }
    }

    public void SetActiveItemByIndex(int index)
    {
        if(_isAttacking) { return; }
        if(index > _items.Length - 1) { return; }
        if(_itemIndex == index) { return; }

        CancelTrapCommerce();
        _itemIndex = index;
        _activeItem = _items[_itemIndex];
        _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
        OnActiveItemChanged?.Invoke(_itemIndex);
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

    void InputManager_OnMainPressed()
    {
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        if(_canBuyTrap && _activeTrap && _activeSocket)
        {
            _audioSource.PlayOneShot(_placeTrapSFX);
            _wallet.SpendMoney(_activeTrap.BuyPrice);
            _activeTrap.CompletePurchase(_activeSocket);
            // TODO : Add an animation (and a cool shader to make the trap appear through magical science)
            return;
        }

        if(!_activeItem.IsTrap)
        {
            if(_isAttacking) { return; }
            if(_myMana.CurrentMana < _activeItem.Cost)
            {
                OnTooTired?.Invoke();
                return;
            }
            RotateModelInstantly();

            _animator.SetTrigger(ATTACK_HASH);

            _isAttacking = true;
        }
    }

    public void Attack()
    {
        _audioSource.PlayOneShot(_attackSFX);
        RotateModelInstantly();
        _activeItem.PrimaryAction(_aimPositionMarker.position);
        _myMana.SpendMana(_activeItem.Cost);
        _isAttacking = false;
    }

    void InputManager_OnSecondaryPressed()
    {
        if(_isAttacking) { return; }
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        if(!_activeItem.IsTrap)
        {
            RotateModelInstantly();
            _activeItem.SecondaryAction();
        }
    }

    void InputManager_OnSprintHeld(bool isHeld)
    {
        _isSprinting = isHeld;
        _animator.SetBool(SPRINT_HASH, isHeld);
    }

    void InputManager_OnSellPressed()
    {
        if(_isAttacking) { return; }
        if(!_inSellMode) { return; }
        if(!_canSellTrap) { return; }
        if(!_activeSocket) { return; }

        _canSellTrap = false;
        OnCanSellTrap?.Invoke(_canSellTrap);
        _activeSocket.SellTrap();
        _audioSource.PlayOneShot(_sellTrapSFX);
    }

    void InputManager_OnScroll(Vector2 value)
    {
        if(_isAttacking) { return; }

        if(value.y > 0)
        {
            InputManager_OnNextPressed();
        }

        if(value.y < 0)
        {
            InputManager_OnPreviousPressed();
        }
    }

    void InputManager_OnPreviousPressed()
    {
        if(_isAttacking) { return; }
        if(_items.Length <= 0) { return; }

        CancelTrapCommerce();
        _itemIndex = (_itemIndex - 1 + _items.Length) % _items.Length;


        _activeItem = _items[_itemIndex];
        _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
        OnActiveItemChanged?.Invoke(_itemIndex);
    }

    void InputManager_OnNextPressed()
    {
        if(_isAttacking) { return; }
        if(_items.Length <= 0) { return; }

        CancelTrapCommerce();
        _itemIndex = (_itemIndex + 1) % _items.Length;

        _activeItem = _items[_itemIndex];
        _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
        OnActiveItemChanged?.Invoke(_itemIndex);
    }

    void _myHealth_OnLoseHealth()
    {
        _audioSource.PlayOneShot(_hurtSFX);
    }

    void MyHealth_OnDeath()
    {
        if(!_isDead)
        {
            _isDead = true;
            _moveInputValue = Vector2.zero;
            _animator.SetTrigger(DEATH_HASH);
        }
    }

    public void DeathComplete()
    {
        Invoke(nameof(Respawn), _respawnDelay);
    }

    void Respawn()
    {
        if(_isLevelOver) { return; }

        transform.position = _respawnPosition;
        _myHealth.ResetHealth();
        _myMana.ResetMana();
        _animator.SetTrigger(RESPAWN_HASH);
        _isDead = false;
    }

    void LevelManager_OnWaveStarted()
    {
        _inSellMode = false;
        _myMana.ResetMana();
        _myHealth.ResetHealth();
    }

    void LevelManager_OnWaveCompleted(int index, int rewards)
    {
        _inSellMode = true;
        _myMana.ResetMana();
        _myHealth.ResetHealth();
    }

    void LevelManager_LevelOver()
    {
        _isLevelOver = true;
        this.enabled = false;   // This seems like the simplest way to disable all input
    }
}
