using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _healthText, _manaText, _moneyText;
    [SerializeField] Slider _healthSlider, _manaSlider;
    [SerializeField] PlayerController _player;
    Health _playerHealth;
    Mana _playerMana;
    Wallet _playerWallet;

    void Awake()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerMana = _player.GetComponent<Mana>();
        _playerWallet = _player.GetComponent<Wallet>();

        _playerHealth.OnHealthChanged += PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged += PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged += PlayerWallet_OnMoneyChanged;
    }

    void OnDestroy()
    {
        _playerHealth.OnHealthChanged -= PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged -= PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged -= PlayerWallet_OnMoneyChanged;
    }

    void PlayerHealth_OnHealthChanged(int currentHealth, int maxHealth)
    {
        _healthText.text = $"{currentHealth} / {maxHealth}";
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = currentHealth;
    }

    void PlayerMana_OnManaChanged(int currentMana, int maxMana)
    {
        _manaText.text = $"{currentMana} / {maxMana}";
        _manaSlider.maxValue = maxMana;
        _manaSlider.value = currentMana;
    }

    void PlayerWallet_OnMoneyChanged(int money)
    {
        _moneyText.text = $"Money : {money}";
    }
}
