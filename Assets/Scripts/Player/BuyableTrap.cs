using UnityEngine;

public class BuyableTrap : Item
{
    [field:SerializeField] public int BuyPrice { get; protected set; } = 100;
    [field:SerializeField] public TrapPosition TrapPosition { get; protected set; } = TrapPosition.Floor;
    [field:SerializeField] public TrapPreview PreviewPrefab { get; private set; }
    
    [SerializeField] Vector2 _size = Vector2.one;
    [SerializeField] Trap _trapPrefab;

    void Awake()
    {
        IsTrap = true;
        Cost = BuyPrice;
    }

    public bool CanPlaceTrap(TrapSocket activeSocket)
    {
        if(activeSocket.HasTrap) { return false; }
        if(activeSocket.SocketPosition != TrapPosition) { return false; }

        // if(_size.x > 1 || _size.y > 1)
        // {
            // TODO ? Assuming larger traps exist, check if Size fits surrounding TrapSockets
            // return [...];
        // }

        return true;
    }

    public void CompletePurchase(TrapSocket activeSocket)
    {
        activeSocket.PlaceTrap(_trapPrefab, BuyPrice);
    }
}
