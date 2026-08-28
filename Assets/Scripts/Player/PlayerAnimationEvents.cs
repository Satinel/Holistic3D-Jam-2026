using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;

    void AttackAnimationEvent()
    {
        _playerController.Attack();
    }

    void DeathAnimationEvent()
    {
        _playerController.DeathComplete();
    }
}
