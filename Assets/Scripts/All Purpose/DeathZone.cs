using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] bool _disableGravity = true;

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInParent<Enemy>())
        {
            other.GetComponentInParent<Health>().Kill();

            if(_disableGravity)
            {
                other.GetComponentInParent<Enemy>().DisableRagdollGravity();
            }
        }
    }
}
