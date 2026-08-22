using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    public event Action<int> ReportTotalItems;
    public event Action<int> OnActiveItemChanged;

    [SerializeField] float _moveSpeed = 2.5f, _rotateSpeed = 1.5f;
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
    [SerializeField] Material _buyMaterial, _poorMaterial;

    Vector2 _moveInputValue = Vector2.zero, _lookAccumulation = Vector2.zero;
    float _currentXAngle = 0f;
    bool _isDead, _inSellMode, _canBuyTrap, _canSellTrap;
    int _itemIndex = 0;
    Item _activeItem;
    BuyableTrap _activeTrap;
    TrapSocket _activeSocket;
    GameObject _previewModel;

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
            ReportTotalItems?.Invoke(_items.Length);
            // TODO ? Rather than sending an int, send the array of items with icons/costs to be set in PlayerHUD
            _activeItem = _items[_itemIndex];
            _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
            OnActiveItemChanged?.Invoke(_itemIndex);
        }
    }

    void Update()
    {
        MovePlayer();
    }

    void LateUpdate()
    {
        RotateCameraTarget();
        _aimPositionMarker.position = _cinemachineThirdPersonAim.AimTarget;
        CheckSocket();
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

    void CheckSocket()
    {
        if(Time.deltaTime == 0) { return; }

        if(!_inSellMode && _activeTrap == null)
        {
            CancelTrapCommerce();
            return;
        }

        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, _socketLayer))
        {
            if(hit.collider.TryGetComponent(out TrapSocket socket))
            {
                _activeSocket = socket;

                if(_activeTrap.CanPlaceTrap(_activeSocket))
                {
                    _activeSocket.HighlightTrap(false);
                    _canSellTrap = false;
                    _canBuyTrap = _wallet.CanAfford(_activeTrap.BuyPrice);

                    if(_previewModel == null)
                    {
                        _previewModel = Instantiate(_activeTrap.PreviewPrefab);;
                    }

                    _previewModel.transform.SetPositionAndRotation(socket.transform.position, socket.transform.rotation);

                    if(_canBuyTrap)
                    {
                        _previewModel.GetComponent<Renderer>().material = _buyMaterial;
                    }
                    else
                    {
                        _previewModel.GetComponent<Renderer>().material = _poorMaterial;
                    }
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
                    // TODO : Activate UI prompt for selling
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
        // TODO : Deactivate UI prompt for selling
        _canBuyTrap = false;
        _canSellTrap = false;
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
            Destroy(_previewModel);
            _previewModel = null;
        }
    }

    public void SetActiveItemByIndex(int index)
    {
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
            _wallet.SpendMoney(_activeTrap.BuyPrice);
            _activeTrap.CompletePurchase(_activeSocket);
            // TODO : Add an animation (and a cool shader to make the trap appear through magical science)
            return;
        }

        if(!_activeItem.IsTrap)
        {
            RotateModelInstantly();
            _activeItem.PrimaryAction(_aimPositionMarker.position);
        }
    }

    void InputManager_OnSecondaryPressed()
    {
        if(_isDead) { return; }
        if(Time.timeScale == 0) { return; }

        if(!_activeItem.IsTrap)
        {
            RotateModelInstantly();
            _activeItem.SecondaryAction();
        }
    }

    void InputManager_OnSellPressed()   // TODO : Keybind for Sell (probably E)
    {
        if(!_inSellMode) { return; }
        if(!_canSellTrap) { return; }
        if(!_activeSocket) { return; }

        _activeSocket.SellTrap();
    }

    void InputManager_OnScroll(Vector2 value)
    {
        if(value.y < 0)
        {
            InputManager_OnNextPressed();
        }

        if(value.y > 0)
        {
            InputManager_OnPreviousPressed();
        }
    }

    void InputManager_OnPreviousPressed()
    {
        if(_items.Length <= 0) { return; }

        CancelTrapCommerce();
        _itemIndex = (_itemIndex + 1) % _items.Length;

        _activeItem = _items[_itemIndex];
        _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
        OnActiveItemChanged?.Invoke(_itemIndex);
    }

    void InputManager_OnNextPressed()
    {
        if(_items.Length <= 0) { return; }

        CancelTrapCommerce();
        _itemIndex = (_itemIndex - 1 + _items.Length) % _items.Length;

        _activeItem = _items[_itemIndex];
        _activeTrap = _activeItem.IsTrap ? (BuyableTrap)_activeItem : null;
        OnActiveItemChanged?.Invoke(_itemIndex);
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
