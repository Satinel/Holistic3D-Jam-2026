using System;
using UnityEngine;

public class TrapSocket : MonoBehaviour
{
    public static event Action<Trap> OnAnyTrapSold;

    [field:SerializeField] public TrapPosition SocketPosition { get; private set; }

    public bool HasTrap { get; private set; }
    Trap _placedTrap = null;

    // void Awake()
    // {
    //     OnAnyTrapSold += RemoveTrap;
    // }

    // void OnDestroy()
    // {
    //     OnAnyTrapSold -= RemoveTrap;
    // }

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
        OnAnyTrapSold?.Invoke(_placedTrap);
        Destroy(_placedTrap.gameObject);
        _placedTrap = null;
    }

    public void HighlightTrap(bool isHighlighted)
    {
        if(!_placedTrap) { return; }

        _placedTrap.HighlightModel.SetActive(isHighlighted);
        _placedTrap.RangeRenderer.enabled = isHighlighted;
    }

    // public void AssignTrap(Trap assignedTrap)   // This was supposed to be for larger traps but somehow this isn't needed? My code is so brilliant even I don't understand it??
    // {
    //     if(HasTrap) { return; }

    //     HasTrap = true;
    //     _placedTrap = assignedTrap;
    // }

    // void RemoveTrap(Trap removedTrap)            // If AssignTrap isn't used then this has no reason to exist either
    // {
    //     if(!HasTrap) { return; }
    //     if(_placedTrap != removedTrap) { return; }

    //     HasTrap = false;
    //     _placedTrap = null;
    // }
}
