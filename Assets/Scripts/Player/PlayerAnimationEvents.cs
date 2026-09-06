using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip[] _footstepsSFX;
    [SerializeField] float _footstepVolume = 0.5f;

    void AttackAnimationEvent()
    {
        _playerController.Attack();
    }

    void FootstepAnimationEvent()
    {
        _audioSource.PlayOneShot(_footstepsSFX[Random.Range(0, _footstepsSFX.Length)], _footstepVolume);
    }

    void DeathAnimationEvent()
    {
        _playerController.DeathComplete();
    }
}
