using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> OnAnyEnemySpawned, OnAnyEnemyDestroyed;
    public static event Action OnBossSpawned;

    [field:SerializeField] public int CoreValue { get; private set; } = 1;

    [SerializeField] int _drainDamage = 10, _ragdollResist = 0;
    [SerializeField] bool _isBoss = false;
    [SerializeField] float _moveSpeed = 2.25f, _acceleration = 10f, _turnSpeed = 7.5f, _destroyDelay = 3f; //_deceleration = 5f;
    [SerializeField] float _ragdollRecoveryTime = 2.5f, _falloffFadeOut = 3f, _stuckCheckTime = 1f;
    [SerializeField] LayerMask _trapLayerMask;
    [SerializeField] Health _health;
    [SerializeField] Collider _mainCollider;
    [SerializeField] Rigidbody _mainRigidbody;
    [SerializeField] Animator _animator;
    [SerializeField] PlayerDetector _playerDetector;
    [SerializeField] FloatingText _floatingTextPrefab;
    [SerializeField] GameObject _armorBreakPrefab;

    [SerializeField] Rigidbody _ragdoll;
    [SerializeField] ModelAnimator _ragdollModel;
    [SerializeField] Collider[] _colliders;
    [SerializeField] Rigidbody[] _rigidbodies;

    [SerializeField] Transform _leftHand, _rightHand;
    [SerializeField] Transform _leftBeam, _rightBeam;
    [SerializeField] NavMeshAgent _navAgent;
    Vector3 _desiredVelocity = new();

    bool _isRagdolled, _isCrushed, _isAttacking;
    float _currentMoveSpeed, _defaultMoveSpeed, _slowMoveSpeed;
    float _ragddollTimer, _ragdollDuration, _crushedTimer, _startingScaleY, _stuckTimer;
    Transform _destination, _deferredDestination;
    BarricadeTrap _targetBarricade;
    Health _playerHealth;
    Core _core;

    public Health EnemyHealth => _health;
    public bool IsAttacking => _isAttacking;
    static readonly int DEATH_HASH = Animator.StringToHash("Death");
    static readonly int ATTACK_HASH = Animator.StringToHash("Attack");
    static readonly int WALKING_NAME_HASH = Animator.StringToHash("Walk");


    void Awake()
    {
        _health.OnDeath += OnDeath;
        OnAnyEnemySpawned?.Invoke(this);
        Health.OnAnyHealthDeath += Health_OnAnyHealthDeath;
    }

    void OnDestroy()
    {
        OnAnyEnemyDestroyed?.Invoke(this);
        _health.OnDeath -= OnDeath;
        Health.OnAnyHealthDeath -= Health_OnAnyHealthDeath;
    }

    void Start()
    {
        _defaultMoveSpeed = _moveSpeed;
        _currentMoveSpeed = _defaultMoveSpeed;
        _slowMoveSpeed = _moveSpeed * 0.5f;
        _startingScaleY = _ragdollModel.transform.localScale.y;

        _navAgent.updatePosition = false;

        _navAgent.speed = _currentMoveSpeed;
        _navAgent.acceleration = _acceleration;

        if(_isBoss)
        {
            OnBossSpawned?.Invoke();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(_ragdollResist > 0) { return; }
        // if(_health.IsDead) { return; }                           // There's no reason not to ragdoll a dead enemy (except for performance but that seems fine)
        if(!collision.gameObject.CompareTag("Trap")) { return; }
        if(collision.gameObject.GetComponent<Trap>()) { return; }   // This should only handle objects without attached Trap monobehaviours (like projectiles)

        float mass = collision.rigidbody ? collision.rigidbody.mass : 1;

        Ragdoll(collision.GetContact(0), collision.relativeVelocity * mass);
    }

    void Update()
    {
        if(!_navAgent.enabled || !_navAgent.isOnNavMesh) { return; }

        _navAgent.isStopped = _isRagdolled || _isCrushed || _isAttacking;

        Vector3 currentVelocity = _mainRigidbody.linearVelocity;
        currentVelocity.y = 0f;
        _navAgent.velocity = currentVelocity;
        _desiredVelocity = _navAgent.isStopped ? Vector3.zero : _navAgent.desiredVelocity;
        _desiredVelocity.y = 0;
    }

    void FixedUpdate()
    {
        if(_health.IsDead) { return; }

        if(_isRagdolled)
        {
            _ragddollTimer += Time.deltaTime;

            if(_ragddollTimer >= _ragdollDuration)
            {
                RecoverFromRagdoll();
            }
        }

        if(_isCrushed)
        {
            _crushedTimer -= Time.deltaTime;

            if(_crushedTimer <= 0)
            {
                RecoverFromCrushed();
            }
        }

        if(_playerHealth && _isAttacking)
        {
            RotateTowardDestination(_playerHealth.AttackTargetPoint);
            PositionBeams(_playerHealth.AttackTargetPoint);
        }
        else
        {
            _leftBeam.gameObject.SetActive(false);
            _rightBeam.gameObject.SetActive(false);
            Move();
        }
    }

    void Move()
    {
        if(!_navAgent.enabled || !_navAgent.isOnNavMesh) { return; }

        _navAgent.nextPosition = _mainRigidbody.position;

        if(!_destination) { return; }

        Vector3 current = _mainRigidbody.linearVelocity;
        Vector3 currentHorizontal = new(current.x, 0, current.z);
        Vector3 velocityDifference = _desiredVelocity - currentHorizontal;
        Vector3 accel = velocityDifference / Time.fixedDeltaTime;
        accel = Vector3.ClampMagnitude(accel, _acceleration);
        _mainRigidbody.AddForce(accel, ForceMode.Acceleration);

        if(!_isRagdolled && !_isCrushed && !_isAttacking && _mainRigidbody.linearVelocity.sqrMagnitude < 0.25f)
        {
            _stuckTimer += Time.deltaTime;
            if(_stuckTimer > _stuckCheckTime)
            {
                HandleStuck();
            }
        }
        else
        {
            _stuckTimer = 0f;
        }
    }

    void RotateTowardDestination(Transform destination)
    {
        if(!destination) { return; }

        Vector3 direction = destination.position - transform.position;
        direction.y = 0;
        Vector3 rotationToFace = direction.normalized;

        if(rotationToFace.sqrMagnitude > 0.001f)
        {
            _mainRigidbody.MoveRotation(Quaternion.Slerp(_mainRigidbody.rotation, Quaternion.LookRotation(rotationToFace, Vector3.up), _turnSpeed * Time.fixedDeltaTime));
        }
    }

    void PositionBeams(Transform player)
    {
        _leftBeam.position = (_leftHand.position + player.position) * 0.5f;
        _leftBeam.up = (player.position - _leftHand.position).normalized;

        _rightBeam.position = (_rightHand.position + player.position) * 0.5f;
        _rightBeam.up = (player.position - _rightHand.position).normalized;
    }

    void HandleStuck()
    {
// Debug.Log("HandleStuck() called");
        Collider[] colliders = new Collider[64];
        Physics.OverlapSphereNonAlloc(_mainRigidbody.position, _navAgent.radius + 0.15f, colliders, _trapLayerMask, QueryTriggerInteraction.Collide);
        foreach(Collider collider in colliders)
        {
            if(collider && collider.TryGetComponent(out BarricadeTrap barricade))
            {
                if(barricade)
                {
                    barricade.SufferAttack(this);
                    _stuckTimer = 0;
                    return;
                }
            }
        }

        _mainRigidbody.position = _navAgent.nextPosition;
        _navAgent.nextPosition = _mainRigidbody.position;
        _mainRigidbody.linearVelocity = Vector3.zero;
        _mainRigidbody.angularVelocity = Vector3.zero;
        _stuckTimer = 0;
    }

    void Ragdoll(ContactPoint contactPoint, Vector3 force)
    {
        if(_isAttacking)
        {
            StopAttack();
            _playerDetector.ToggleActive(false);
        }

        _ragdollDuration = _ragdollDuration < _ragdollRecoveryTime ? _ragdollRecoveryTime : _ragdollDuration;
        _ragddollTimer = 0;

        if(_isRagdolled) { return; }

        _isRagdolled = true;
        _animator.enabled = false;
        _mainCollider.enabled = false;
        _mainRigidbody.isKinematic = true;
        _navAgent.enabled = false;

        Rigidbody closestBone = null;
        float smallestDistance = Mathf.Infinity;

        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;

            float distance = (rigidbody.worldCenterOfMass - contactPoint.point).sqrMagnitude;
            closestBone = distance < smallestDistance ? rigidbody : closestBone;
        }

        if(closestBone != null)
        {
            closestBone.AddForceAtPosition(force, contactPoint.point, ForceMode.Impulse);

            foreach(Rigidbody rigidbody in _rigidbodies)
            {
                if(rigidbody == closestBone) { continue; }

                float distance = Vector3.Distance(rigidbody.worldCenterOfMass, contactPoint.point);
                float falloff = Mathf.Clamp01(1f - distance / _falloffFadeOut);
                rigidbody.AddForceAtPosition(force * falloff, contactPoint.point, ForceMode.Impulse);
            }
        }

        foreach(Collider collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    public void AccurateRagdoll(Vector3 force, ForceMode forceMode, float ragdollDuration)
    {
        if(_ragdollResist > 0)
        {
            _ragdollResist--;

            if(_ragdollResist == 0)
            {
                BreakArmor();
            }
            return;
        }

        if(_isAttacking)
        {
            StopAttack();
            _playerDetector.ToggleActive(false);
        }

        _ragdollDuration = ragdollDuration > _ragdollDuration ? ragdollDuration : _ragdollDuration;
        _ragddollTimer = 0;
        _isRagdolled = true;
        _animator.enabled = false;
        _mainCollider.enabled = false;
        _mainRigidbody.isKinematic = true;
        _navAgent.enabled = false;

        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(force, forceMode);
        }
        foreach(Collider collider in _colliders)
        {
            collider.enabled = true;
        }
    }

    void RecoverFromRagdoll()
    {
        RecoverFromSlow();
        Vector3 ragdollPosition = _ragdoll.position;
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        foreach(Collider collider in _colliders)
        {
            collider.enabled = false;
        }
        _mainRigidbody.position = ragdollPosition;

        _navAgent.enabled = true;
        _navAgent.Warp(_mainRigidbody.position);

        _navAgent.ResetPath();
        _navAgent.destination = _destination.position;

        // TODO : Check if this position is inside a non-trigger collider and move it out if so (otherwise Enemies get sucked through walls)
        _mainCollider.enabled = true;
        _mainRigidbody.isKinematic = false;

        if(!_isCrushed)
        {
            _animator.enabled = true;
            _playerDetector.ToggleActive(true);
        }
        // TODO (but probably won't have time in a game jam) : Set to a stand up animation based on supine/prone position

        _isRagdolled = false;
        _ragdollDuration = 0;
        _ragddollTimer = 0;
    }

    public void Crush(float newScaleY, float duration)
    {
        if(_ragdollResist > 0)
        {
            _ragdollResist--;
            if(_ragdollResist == 0)
            {
                BreakArmor();
            }
        }
        if(_isAttacking)
        {
            _playerDetector.ToggleActive(false);
            StopAttack();
        }

        if(_isRagdolled) { return; }

        _animator.enabled = false;
        _crushedTimer = duration;
        _ragdollModel.transform.localScale = new(_ragdollModel.transform.localScale.x, newScaleY, _ragdollModel.transform.localScale.z);
        _isCrushed = true;
    }

    void RecoverFromCrushed()
    {
        RecoverFromSlow();
        _ragdollModel.transform.localScale = new(_ragdollModel.transform.localScale.x, _startingScaleY, _ragdollModel.transform.localScale.z);
        // TODO : Maybe play Pop sound effect from 2D Princess here
        _isCrushed = false;
        if(!_isRagdolled)
        {
            _animator.enabled = true;
            _playerDetector.ToggleActive(true);
        }
    }

    public void Slow()
    {
        _currentMoveSpeed = _slowMoveSpeed;
        _animator.speed = 0.5f;
        _navAgent.speed = _currentMoveSpeed;
        // TODO ? If enemies ever make noises or have voice lines, lower pitch by * 0.5f
    }

    public void RecoverFromSlow()
    {
        _currentMoveSpeed = _defaultMoveSpeed;
        _animator.speed = 1f;
        _navAgent.speed = _currentMoveSpeed;
        // TODO ? If enemies ever make noises or have voice lines, restore pitch level which was lowered in Slow()
    }

    void BreakArmor()
    {
        Instantiate(_armorBreakPrefab, _ragdoll.transform.position, Quaternion.identity);
    }

    public void DisableRagdollGravity()
    {
        foreach(Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.useGravity = false;
        }
    }

    public void SetDestination(Transform destination)
    {
        if(!destination) { return; }

        if(_targetBarricade != null)
        {
            _deferredDestination = destination; // Waypoints shouldn't overwrite destination if headed to a blocking barricade
            return;
        }

        _destination = destination;
        if(_navAgent.isOnNavMesh)
        {
            _navAgent.destination = _destination.position;
        }
    }

    public void SetCore(Core core)
    {
        _core = core;
    }

    public void SetTargetBarricade(BarricadeTrap barricade, Transform attackPoint)
    {
        if(_health.IsDead) { return; }

        if(barricade == null)   // If barricade is passed as null it means there should be a path to _core
        {
            _targetBarricade = null;
            if(_deferredDestination)
            {
                SetDestination(_deferredDestination);
                _deferredDestination = null;
                return;
            }
            else if(_destination)
            {
                SetDestination(_destination);
                return;
            }
            else
            {
                SetDestination(_core.transform);
                return;
            }
        }

        NavMeshPath path = new();
        if(NavMesh.CalculatePath(transform.position, _core.transform.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete) // Ignore if path to _core is not blocked
        {
            _targetBarricade = null;
            if(_deferredDestination)
            {
                SetDestination(_deferredDestination);
                _deferredDestination = null;
                return;
            }
            else if(_destination)
            {
                SetDestination(_destination);
                return;
            }
            else
            {
                SetDestination(_core.transform);
                return;
            }
        }
        else if(_targetBarricade)
        {
            for(int i = 0; i < _targetBarricade.AttackPoints.Length; i++)
            {
                if(NavMesh.CalculatePath(transform.position, _targetBarricade.AttackPoints[i].position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    BarricadeTrap target = _targetBarricade;    // Yes this is convoluted but less so than introducing a bool to track everywhere all the time
                    _targetBarricade = null;
                    SetDestination(target.AttackPoints[i]);
                    _targetBarricade = target;
                    return; // Ignore if heading to a different blocking and reachable barricade already (so Enemy doesn't get trapped heading for the wrong side of a different barricade)
                }
            }
        }

        if(!_targetBarricade)
        {
            _deferredDestination = _destination;
        }
        _targetBarricade = null;
        SetDestination(attackPoint);
        _targetBarricade = barricade;
    }

    public void AttackBarricade(BarricadeTrap barricade)
    {
        // TODO Trigger animation and such
        barricade.SufferAttack(this);
    }

    public void StartAttack(Health playerHealth)
    {
        if(_isRagdolled || _isCrushed || _isAttacking) { return; }

        _navAgent.enabled = false;
        _playerHealth = playerHealth;
        _isAttacking = true;
        _animator.SetBool(ATTACK_HASH, true);
    }

    public void DealDamage()
    {
        if(!_playerHealth) { return; }

        _leftBeam.gameObject.SetActive(true);
        _rightBeam.gameObject.SetActive(true);
        _playerHealth.LoseHealth(_drainDamage);
    }

    public void StopAttack()
    {
        _animator.SetBool(ATTACK_HASH, false);
        _animator.Play(WALKING_NAME_HASH);
        _isAttacking = false;
        _leftBeam.gameObject.SetActive(false);
        _rightBeam.gameObject.SetActive(false);

        if(!_isRagdolled)
        {
            _navAgent.enabled = true;
            _navAgent.ResetPath();
            _navAgent.destination = _destination.position;
        }
    }

    void Health_OnAnyHealthDeath(Health health)
    {
        if(_playerHealth && health == _playerHealth)
        {
            StopAttack();
        }
    }

    void OnDeath()
    {
        if(!_isRagdolled && !_isCrushed)
        {
            _animator.SetTrigger(DEATH_HASH);
        }

        // TODO ? A really fancy shader should make the model disintegrate or something!!!
        FloatingText floatingText = Instantiate(_floatingTextPrefab, _ragdoll.position, Quaternion.identity);
        floatingText.SetUp(_health.MoneyValue.ToString());
        _navAgent.enabled = false;
        Destroy(gameObject, _destroyDelay);
    }
}
