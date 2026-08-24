using UnityEngine;

public class Item : MonoBehaviour
{
    [field:SerializeField] public bool IsTrap { get; protected set; }
    [field:SerializeField] public int Cost { get; protected set; } = 5;
    [field:SerializeField] public Sprite Icon { get; protected set; }

    public virtual void PrimaryAction(Vector3 direction){}
    public virtual void SecondaryAction(){}
}
