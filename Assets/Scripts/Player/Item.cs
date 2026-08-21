using UnityEngine;

public class Item : MonoBehaviour
{
    [field:SerializeField] public bool IsTrap { get; protected set; }

    public virtual void PrimaryAction(){}
    public virtual void SecondaryAction(){}
}
