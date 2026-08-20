using UnityEngine;

public enum TrapPosition
{
    Floor,
    Wall,
    Ceiling,
}

public class Trap : MonoBehaviour
{
    [field:SerializeField] public int Cost { get; protected set; } = 100;
    [field:SerializeField] public TrapPosition TrapPosition { get; protected set; } = TrapPosition.Floor;
    [field:SerializeField] public Vector2 Size { get; protected set; } = Vector2.one;

    [field:SerializeField] public int Damage { get; protected set; } = 5;
    [field:SerializeField] public float RagdollDuration { get; protected set; } = 4.5f;
    [field:SerializeField] public bool UsesPhysics { get; protected set; } = true;
    [field:SerializeField] public bool OverridesPhysics { get; protected set; } = false;
    [field:SerializeField] public float ForceOverride { get; protected set; } = 10f;
    [field:SerializeField] public ForceMode ForceMode { get; protected set; } = ForceMode.Impulse;

    public virtual void TrapAction(Enemy enemy){}
}
