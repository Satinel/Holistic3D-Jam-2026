using System;
using UnityEngine;

public class TrapSocket : MonoBehaviour
{
    public static event Action<int> OnAnyTrapSold;

    [field:SerializeField] public TrapPosition SocketPosition { get; private set; }

    public bool HasTrap { get; private set; }
    Trap _placedTrap = null;

    public void PlaceTrap(Trap trapPrefab, int trapPrice)
    {
        if(HasTrap) { return; }

        HasTrap = true;
        _placedTrap = Instantiate(trapPrefab, transform.position, transform.rotation, transform);
        _placedTrap.Initialize(trapPrice);
    }

    public void SellTrap()
    {
        if(!HasTrap) { return; }

        HasTrap = false;
        OnAnyTrapSold?.Invoke(_placedTrap.SellPrice);
        Destroy(_placedTrap.gameObject);
        _placedTrap = null;
    }

    public void HighlightTrap(bool isHighlighted)
    {
        if(!_placedTrap) { return; }

        _placedTrap.HighlightModel.SetActive(isHighlighted);
        _placedTrap.RangeRenderer.enabled = isHighlighted;
    }
}
