using System;
using System.Collections.Generic;
using UnityEngine;

public class TrapSocket : MonoBehaviour
{
    public static event Action<Trap> OnAnyTrapSold;

    [field:SerializeField] public TrapPosition SocketPosition { get; private set; }

    public bool HasTrap { get; private set; }
    Trap _placedTrap = null;
    SocketBlocker _firstSocketBlocker = null;
    HashSet<SocketBlocker> _socketBlockers = new();
    public bool IsBlocked => _socketBlockers.Count > 0;

    public void PlaceTrap(Trap trapPrefab, int trapPrice, Quaternion rotation)
    {
        if(HasTrap) { return; }

        HasTrap = true;
        _placedTrap = Instantiate(trapPrefab, transform.position, rotation, transform);
        _placedTrap.Initialize(trapPrice, this);
    }

    public void SellTrap()
    {
        if(HasTrap)
        {
            HasTrap = false;
            OnAnyTrapSold?.Invoke(_placedTrap);
            Destroy(_placedTrap.gameObject);
            _placedTrap = null;
        }
        else if(IsBlocked)
        {
            _firstSocketBlocker.BlockingTrap.ForceSale();
        }
    }

    public void TrapDestroyed()
    {
        if(HasTrap)
        {
            HasTrap = false;
        }
        if(_placedTrap)
        {
            _placedTrap = null;
        }
    }

    public void HighlightTrap(bool isHighlighted)
    {
        if(!_placedTrap && !IsBlocked) { return; }

        if(!IsBlocked)
        {
            _placedTrap.HighlightModel.SetActive(isHighlighted);
            _placedTrap.RangeRenderer.enabled = isHighlighted;
        }
        else
        {
            _firstSocketBlocker.BlockingTrap.HighlightModel.SetActive(isHighlighted);
            _firstSocketBlocker.BlockingTrap.RangeRenderer.enabled = isHighlighted;
        }
    }

    public void Block(SocketBlocker socketBlocker)
    {
        _socketBlockers.Add(socketBlocker);
        if(!_firstSocketBlocker)
        {
            _firstSocketBlocker = socketBlocker;
        }
    }

    public void Unblock(SocketBlocker socketBlocker)
    {
        _socketBlockers.Remove(socketBlocker);

        if(_firstSocketBlocker == socketBlocker)
        {
            if(_socketBlockers.Count > 0)
            {
                foreach(SocketBlocker blocker in _socketBlockers)
                {
                    _firstSocketBlocker = blocker;
                    break;
                }
            }
            else
            {
                _firstSocketBlocker = null;
            }
        }
    }
}
