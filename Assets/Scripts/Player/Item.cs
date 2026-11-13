using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public event Action OnItemUsed;

    [field:SerializeField] public bool IsTrap { get; protected set; }
    [field:SerializeField] public bool IsUnlocked { get; protected set; }
    [field:SerializeField] public int Damage { get; protected set; } = 5;
    [field:SerializeField] public int Cost { get; protected set; } = 5;
    [field:SerializeField] public Sprite Icon { get; protected set; }
    [field:SerializeField] public string ItemName { get; protected set; } = string.Empty;
    [field:SerializeField] public string Description { get; protected set; } = string.Empty;

    public int Index { get; protected set; } = -1;
    public int Slot { get; protected set; } = -1;
    public bool WasUsed { get; protected set; }

    public virtual void PrimaryAction(Vector3 direction){}
    public virtual void SecondaryAction(){}

    public void Unlock(bool isUnlocked)
    {
        IsUnlocked = isUnlocked;
    }

    public void SetIndex(int newIndex)
    {
        Index = newIndex;
    }

    public void SetItemSlot(int newSlot)
    {
        Slot = newSlot;
    }

    public void SetWasUsed(bool wasUsed)
    {
        WasUsed = wasUsed;
        OnItemUsed?.Invoke();
    }
}
