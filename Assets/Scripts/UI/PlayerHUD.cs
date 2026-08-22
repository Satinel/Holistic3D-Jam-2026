using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _healthText, _manaText, _moneyText, _coreText;
    [SerializeField] Slider _healthSlider, _manaSlider;
    [SerializeField] PlayerController _player;
    [SerializeField] GameObject[] _icons;
    [SerializeField] Image[] _iconHighlights;

    Health _playerHealth;
    Mana _playerMana;
    Wallet _playerWallet;

    void Awake()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerMana = _player.GetComponent<Mana>();
        _playerWallet = _player.GetComponent<Wallet>();

        _player.ReportTotalItems += Player_ReportTotalItems;
        _player.OnActiveItemChanged += Player_OnActiveItemChanged;
        _playerHealth.OnHealthChanged += PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged += PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged += PlayerWallet_OnMoneyChanged;
        Core.OnCoreValueChanged += Core_OnCoreValueChanged;
    }

    void OnDestroy()
    {
        _player.ReportTotalItems -= Player_ReportTotalItems;
        _player.OnActiveItemChanged -= Player_OnActiveItemChanged;
        _playerHealth.OnHealthChanged -= PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged -= PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged -= PlayerWallet_OnMoneyChanged;
        Core.OnCoreValueChanged -= Core_OnCoreValueChanged;
    }

    void Player_ReportTotalItems(int totalItems)
    {
        for(int i = 0; i < totalItems; i++)
        {
            _icons[i].SetActive(true);
        }
    }

    void Player_OnActiveItemChanged(int index)
    {
        if(index >= _iconHighlights.Length) { return; }

        foreach(Image highlight in _iconHighlights)
        {
            highlight.enabled = false;
        }

        _iconHighlights[index].enabled = true;
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

    void Core_OnCoreValueChanged(int value)
    {
        _coreText.text = value.ToString();
    }
}
