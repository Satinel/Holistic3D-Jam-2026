using UnityEngine;

public class ModelAnimator : MonoBehaviour
{
    [SerializeField] Enemy _parent;

    void DeathAnimationEvent()
    {
        Destroy(_parent.gameObject);
    }
}
