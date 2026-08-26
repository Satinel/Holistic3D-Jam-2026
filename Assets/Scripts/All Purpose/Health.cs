using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public static event Action<Health> OnAnyHealthDeath;
    public event Action OnLoseHealth;
    public event Action<Vector3, float> OnKnockBack;
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    // [field:SerializeField] public Collider Collider { get; private set; }
    // [field:SerializeField] public bool IsEnemy { get; private set; }
    [field:SerializeField] public bool IsPlayer { get; private set; }
    [field:SerializeField] public int MoneyValue { get; private set; } = 10;

    [SerializeField] int _maxHealth = 100, _healthPerSecond = 0;

    float _regenTimer;
    int _currentHealth;
    bool _isDead, _isInvincible;
    public bool IsDead => _isDead;
    public bool IsInvincible => _isInvincible;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;

    void Start()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    void Update()
    {
        if(_isDead) { return; }

        if(_healthPerSecond > 0 && _currentHealth < _maxHealth)
        {
            _regenTimer += Time.deltaTime;

            if(_regenTimer > 1)
            {
                _regenTimer -= 1;
                GainHealth(_healthPerSecond);
            }
        }
    }

    public void LoseHealth(int lostAmount)
    {
        if(_isDead || lostAmount <= 0) { return; }

        _currentHealth = _currentHealth - lostAmount < 0 ? 0 : _currentHealth - lostAmount;

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        OnLoseHealth?.Invoke();

        if(_currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void LoseHealthWithDisplay(int lostAmount, FloatingText floatingTextPrefab, Vector3 textPosition, Color textColor)
    {
        if(_isDead) { return; }

        if(_isInvincible)
        {
            lostAmount = 0;
        }

        FloatingText floatingText = Instantiate(floatingTextPrefab, textPosition, Quaternion.identity);
        floatingText.SetUp(lostAmount.ToString(), textColor);

        _currentHealth = Mathf.Max(_currentHealth - lostAmount, 0);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        OnLoseHealth?.Invoke();

        if(_currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    public void KnockBack(Vector3 force, float duration)
    {
        OnKnockBack?.Invoke(force, duration);
    }

    public void GainHealth(int gainedAmount)
    {
        _currentHealth = Mathf.Min(_currentHealth + gainedAmount, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        // TODO : (Green) Floating Text
    }

    public void ResetHealth()
    {
        GainHealth(_maxHealth);
        _isDead = false;
    }

    void HandleDeath()
    {
        if(_isDead) { return; }

        _isDead = true;

        OnAnyHealthDeath?.Invoke(this);
        OnDeath?.Invoke();
    }

    public void SetInvincibility(bool isInvincible)
    {
        _isInvincible = isInvincible;
    }

    public void Kill()
    {
        if(IsPlayer) { return; }    // This should only be called by things which instantly kill enemies, never the player

        HandleDeath();
    }
}
