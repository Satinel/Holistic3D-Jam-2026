using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _healthText, _manaText, _moneyText, _coreText;
    [SerializeField] Slider _healthSlider, _manaSlider;
    [SerializeField] PlayerController _player;
    [SerializeField] GameObject[] _icons;
    [SerializeField] Image[] _iconImages;
    [SerializeField] Image[] _iconHighlights;
    [SerializeField] GameObject _nextWaveMessage, _canSellMessage;

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
        _player.OnCanSellTrap += Player_OnCanSellTrap;
        _playerHealth.OnHealthChanged += PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged += PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged += PlayerWallet_OnMoneyChanged;
        Core.OnCoreValueChanged += Core_OnCoreValueChanged;
        LevelManager.OnWaveStarted += LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
    }

    void OnDestroy()
    {
        _player.ReportTotalItems -= Player_ReportTotalItems;
        _player.OnActiveItemChanged -= Player_OnActiveItemChanged;
        _player.OnCanSellTrap -= Player_OnCanSellTrap;
        _playerHealth.OnHealthChanged -= PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged -= PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged -= PlayerWallet_OnMoneyChanged;
        Core.OnCoreValueChanged -= Core_OnCoreValueChanged;
        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
    }

    void Player_ReportTotalItems(Item[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            _icons[i].SetActive(true);
            _iconImages[i].sprite = items[i].Icon;
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

    void Player_OnCanSellTrap(bool canSell)
    {
        _canSellMessage.SetActive(canSell);
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

    void LevelManager_OnWaveStarted()
    {
        _nextWaveMessage.SetActive(false);
    }

    void LevelManager_OnWaveCompleted(int _)
    {
        _nextWaveMessage.SetActive(true);
    }
}
