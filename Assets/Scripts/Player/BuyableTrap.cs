using UnityEngine;

public class BuyableTrap : Item
{
    [field:SerializeField] public TrapPosition TrapPosition { get; protected set; } = TrapPosition.Floor;
    [field:SerializeField] public TrapPreview TrapPreview { get; private set; }
    
    [SerializeField] LayerMask _socketLayer;
    [SerializeField] Trap _trapPrefab;

    void Awake()
    {
        IsTrap = true;
    }

    public bool CanPlaceTrap(TrapSocket activeSocket)
    {
        if(activeSocket.SocketPosition != TrapPosition) { return false; }
        if(activeSocket.IsBlocked) { return false; }
        if(activeSocket.HasTrap) { return false; }

        TrapPreview.gameObject.SetActive(true);

        TrapPreview.transform.SetPositionAndRotation(activeSocket.transform.position, TrapPreview.GetRotation(activeSocket.transform.rotation));

        if(TrapPreview.SocketPoints.Length == 0) { return true; }

        foreach(Transform point in TrapPreview.SocketPoints)
        {
            if(Physics.Raycast(point.position, point.forward, out RaycastHit hitInfo, 0.5f, _socketLayer, QueryTriggerInteraction.Collide))
            {
                if(hitInfo.collider.TryGetComponent(out TrapSocket socket))
                {
                    if(socket.HasTrap) { TrapPreview.SetMaterials(false); return false; }
                    if(socket.IsBlocked) { TrapPreview.SetMaterials(false); return false; }
                    if(socket.SocketPosition != TrapPosition) { TrapPreview.SetMaterials(false); return false; }
                }
                else
                {
                    TrapPreview.SetMaterials(false);
                    return false;
                }
            }
            else
            {
                TrapPreview.SetMaterials(false);
                return false;
            }
        }

        return true;
    }

    public void DisableTrapPreview()
    {
        if(TrapPreview)
        {
            TrapPreview.gameObject.SetActive(false);
        }
    }

    public void CompletePurchase(TrapSocket activeSocket)
    {
        DisableTrapPreview();
        activeSocket.PlaceTrap(_trapPrefab, Cost, TrapPreview.transform.rotation);
    }

    public override void SecondaryAction()
    {
        base.SecondaryAction();
        TrapPreview.SetRotation();
    }
}
