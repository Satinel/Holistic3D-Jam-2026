using UnityEngine;

public class Item : MonoBehaviour
{
    [field:SerializeField] public bool IsTrap { get; protected set; }
    [field:SerializeField] public bool IsUnlocked { get; protected set; }
    [field:SerializeField] public int Cost { get; protected set; } = 5;
    [field:SerializeField] public Sprite Icon { get; protected set; }
    [field:SerializeField] public string ItemName { get; protected set; } = string.Empty;

    public virtual void PrimaryAction(Vector3 direction){}
    public virtual void SecondaryAction(){}

    public void Unlock(bool isUnlocked)
    {
        IsUnlocked = isUnlocked;
    }
}
