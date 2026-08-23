using UnityEngine;

public enum TrapPosition
{
    Floor,
    Wall,
    Ceiling,
}

public class Trap : MonoBehaviour
{
    public static readonly int TRIGGER_HASH = Animator.StringToHash("Trigger");
    public static readonly string ENEMY_TAG = "Enemy";

    [field:SerializeField] public int SellPrice { get; protected set; } = 100;
    // [field:SerializeField] public TrapPosition TrapPosition { get; protected set; } = TrapPosition.Floor;
    // [field:SerializeField] public Vector2 Size { get; protected set; } = Vector2.one;
    [field:SerializeField] public Renderer RangeRenderer { get; protected set; }
    [field:SerializeField] public GameObject HighlightModel { get; protected set; }

    [field:SerializeField] public float RechargeTime { get; protected set; } = 2.5f;
    [field:SerializeField] public int Damage { get; protected set; } = 5;
    [field:SerializeField] public float RagdollDuration { get; protected set; } = 4.5f;
    // [field:SerializeField] public bool UsesPhysics { get; protected set; } = true;
    // [field:SerializeField] public bool OverridesPhysics { get; protected set; } = false;
    // [field:SerializeField] public Vector3 DirectionOverride { get; protected set; } = Vector3.one;
    // [field:SerializeField] public float ForceOverride { get; protected set; } = 10f;
    [field:SerializeField] public ForceMode ForceMode { get; protected set; } = ForceMode.Impulse;

    [SerializeField] protected Animator _animator;


    public void Initialize(int price)
    {
        SellPrice = price;
    }
}
