using UnityEngine;

public class BuyableTrap : Item
{
    [field:SerializeField] public int BuyPrice { get; protected set; } = 100;
    [field:SerializeField] public TrapPosition TrapPosition { get; protected set; } = TrapPosition.Floor;
    [field:SerializeField] public TrapPreview PreviewPrefab { get; private set; }
    
    [SerializeField] LayerMask _socketLayer;
    [SerializeField] Vector3 _halfSize = new(1f, 0.01f, 0.9f);
    [SerializeField] Trap _trapPrefab;
    [SerializeField] float _requiredHorizontalSockets = 1, _requiredVerticalSockets = 1;

    void Awake()
    {
        IsTrap = true;
        Cost = BuyPrice;
    }

    public bool CanPlaceTrap(TrapSocket activeSocket)
    {
        if(activeSocket.HasTrap) { return false; }
        if(activeSocket.SocketPosition != TrapPosition) { return false; }

        Collider[] adjacentSockets = Physics.OverlapBox(activeSocket.transform.position, _halfSize, activeSocket.transform.rotation, _socketLayer, QueryTriggerInteraction.Collide);
        foreach(Collider collider in adjacentSockets)
        {
            if(collider.TryGetComponent(out TrapSocket socket))
            {
                if(socket.SocketPosition != TrapPosition) { continue; }
                if(socket.HasTrap) { return false; }
            }
        }

        if(_requiredHorizontalSockets > 1)
        {
            // Check if enough free sockets exist
            //else return false;
        }

        if(_requiredVerticalSockets > 1)
        {
            // Check if enough free sockets exist
            //else return false;
        }

        return true;
    }

    public void CompletePurchase(TrapSocket activeSocket)
    {
        activeSocket.PlaceTrap(_trapPrefab, BuyPrice);
    }
}
