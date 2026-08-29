using UnityEngine;

public class Tigey : MonoBehaviour
{
    [SerializeField] int _moneyValue = 50;
    [SerializeField] FloatingText _floatingTextPrefab;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(PlayerDetector.PLAYER_TAG))
        {
            if(other.TryGetComponent(out Wallet wallet))
            {
                wallet.GainMoney(_moneyValue);
                FloatingText floatingText = Instantiate(_floatingTextPrefab, transform.position, Quaternion.identity);
                floatingText.SetUp($"{_moneyValue}");
                Destroy(gameObject);
            }
        }
    }
}
