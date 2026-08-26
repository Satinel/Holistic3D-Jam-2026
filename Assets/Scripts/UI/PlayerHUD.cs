using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _healthText, _manaText, _moneyText, _coreText;
    [SerializeField] Slider _healthSlider, _manaSlider;
    [SerializeField] PlayerController _player;
    [SerializeField] GameObject[] _icons;
    [SerializeField] Image[] _iconImages;
    [SerializeField] TextMeshProUGUI[] _iconCostTexts;
    [SerializeField] Image[] _iconHighlights;
    [SerializeField] GameObject _nextWaveMessage, _canSellMessage;
    [SerializeField] Color _trapColor, _baseColor;
    [SerializeField] Image _painVignette;
    Coroutine _vignetteRoutine;
    Color _vignetteStartColor;

    Health _playerHealth;
    Mana _playerMana;
    Wallet _playerWallet;
    float _wait = 0.1f;
    WaitForSeconds _waitForSeconds;

    void Awake()
    {
        _playerHealth = _player.GetComponent<Health>();
        _playerMana = _player.GetComponent<Mana>();
        _playerWallet = _player.GetComponent<Wallet>();

        _player.ReportTotalItems += Player_ReportTotalItems;
        _player.OnActiveItemChanged += Player_OnActiveItemChanged;
        _player.OnCanSellTrap += Player_OnCanSellTrap;
        _playerHealth.OnLoseHealth += Player_OnLoseHealth;
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
        _playerHealth.OnLoseHealth -= Player_OnLoseHealth;
        _playerHealth.OnHealthChanged -= PlayerHealth_OnHealthChanged;
        _playerMana.OnManaChanged -= PlayerMana_OnManaChanged;
        _playerWallet.OnMoneyChanged -= PlayerWallet_OnMoneyChanged;
        Core.OnCoreValueChanged -= Core_OnCoreValueChanged;
        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
    }

    void Start()
    {
        _waitForSeconds = new(_wait);
        _vignetteStartColor = _painVignette.color;
        _painVignette.enabled = false;
    }

    void Player_ReportTotalItems(Item[] items)
    {
        for(int i = 0; i < items.Length; i++)
        {
            _icons[i].SetActive(true);
            _iconImages[i].sprite = items[i].Icon;
            _iconCostTexts[i].text = items[i].Cost <= 0 ? string.Empty : items[i].Cost.ToString();
            _iconCostTexts[i].color = items[i].IsTrap ? _trapColor : _baseColor;
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

    void Player_OnLoseHealth()
    {
        if(_vignetteRoutine != null)
        {
            _vignetteRoutine = null;
        }
        _painVignette.color = _vignetteStartColor;
        _painVignette.enabled = true;
        _vignetteRoutine = StartCoroutine(VignetteRoutine());
    }

    IEnumerator VignetteRoutine()
    {
        while(_painVignette.color.a > 0)
        {
            _painVignette.color = new Color(_painVignette.color.r, _painVignette.color.g, _painVignette.color.b, _painVignette.color.a - 0.025f);
            yield return _waitForSeconds;
        }
        _painVignette.enabled = false;
        _vignetteRoutine = null;
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
