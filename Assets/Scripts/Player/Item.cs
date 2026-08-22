using UnityEngine;

public class Item : MonoBehaviour
{
    [field:SerializeField] public bool IsTrap { get; protected set; }

    public virtual void PrimaryAction(Vector3 direction){}
    public virtual void SecondaryAction(){}
}
