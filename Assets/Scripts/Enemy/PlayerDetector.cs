using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] Enemy _thisEnemy;
    [SerializeField] Collider _collider;
    // [SerializeField] GameObject _glowyField;

    bool _isActive = true;

    public static readonly string PLAYER_TAG = "Player";

    void OnTriggerEnter(Collider other)
    {
        if(!_isActive) { return; }

        if(other.CompareTag(PLAYER_TAG))
        {
            if(other.TryGetComponent(out Health health))
            {
                if(health.IsPlayer && !health.IsDead)
                {
                    _thisEnemy.StartAttack(health);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag(PLAYER_TAG))
        {
            if(other.TryGetComponent(out Health health))
            {
                if(health.IsPlayer)
                {
                    _thisEnemy.StopAttack();
                }
            }
        }
    }

    public void ToggleActive(bool isActive)
    {
        _isActive = isActive;
        _collider.enabled = isActive;
    }
}
