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

    readonly Collider[] _adjacentSockets = new Collider[36];

    void Awake()
    {
        IsTrap = true;
        Cost = BuyPrice;
    }

    public bool CanPlaceTrap(TrapSocket activeSocket)
    {
        if(activeSocket.IsBlocked) { return false; }
        if(activeSocket.HasTrap) { return false; }
        if(activeSocket.SocketPosition != TrapPosition) { return false; }

        int socketCount = Physics.OverlapBoxNonAlloc(activeSocket.transform.position, _halfSize, _adjacentSockets, activeSocket.transform.rotation, _socketLayer, QueryTriggerInteraction.Collide);
        for(int i = 0; i < socketCount; i++)
        {
            if(_adjacentSockets[i].TryGetComponent(out TrapSocket socket))
            {
                if(socket.SocketPosition != TrapPosition) { continue; }
                if(socket.HasTrap) { return false; }
            }
        }

        if(_requiredHorizontalSockets > 1)
        {
            for(int i = 0; i < _requiredHorizontalSockets + 1; i++) // I almost understand why the +1 is needed, but in any case it's definitely needed
            {
                float xFactor = i % 2 == 0 ? i : -(i - 1);
                Vector3 origin = activeSocket.transform.TransformPoint(new Vector3(xFactor, -0.5f, 0));

                if(!Physics.Raycast(origin, activeSocket.transform.up, out RaycastHit hit, 0.51f, _socketLayer, QueryTriggerInteraction.Collide)) { return false; }
                if(!hit.collider.TryGetComponent(out TrapSocket trapSocket)) { return false; }
                if(trapSocket.HasTrap) { return false; }
                if(trapSocket.IsBlocked) { return false; }
            }
        }

        if(_requiredVerticalSockets > 1)
        {
            for(int i = 2; i < _requiredVerticalSockets + 1; i++)
            {
                float zFactor = i % 2 == 0 ? i : -(i - 1);
                Vector3 origin = activeSocket.transform.TransformPoint(new Vector3(0, -0.5f, zFactor));

                if(!Physics.Raycast(origin, activeSocket.transform.up, out RaycastHit hit, 0.51f, _socketLayer, QueryTriggerInteraction.Collide)) { return false; }
                if(!hit.collider.TryGetComponent(out TrapSocket trapSocket)) { return false; }
                if(trapSocket.HasTrap) { return false; }
                if(trapSocket.IsBlocked) { return false; }
            }
        }

        return true;
    }

    public void CompletePurchase(TrapSocket activeSocket)
    {
        activeSocket.PlaceTrap(_trapPrefab, BuyPrice);
    }
}
