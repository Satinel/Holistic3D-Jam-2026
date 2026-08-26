using UnityEngine;

public class ModelAnimator : MonoBehaviour
{
    [SerializeField] Enemy _parent;

    void AttackAnimationEvent()
    {
        _parent.DealDamage();
    }

    // void DeathAnimationEvent()   // Destroy is called in Enemy regardless of whether DeathAnimation occurs
    // {
    //     Destroy(_parent.gameObject);
    // }
}
