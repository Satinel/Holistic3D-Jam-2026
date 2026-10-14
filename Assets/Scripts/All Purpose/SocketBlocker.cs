using System.Collections.Generic;
using UnityEngine;

public class SocketBlocker : MonoBehaviour
{
    [field:SerializeField] public Trap BlockingTrap { get; private set; }
    readonly HashSet<TrapSocket> _blockedSockets = new();

    void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out TrapSocket socket))
        {
            _blockedSockets.Add(socket);
            socket.Block(this);
        }
    }

    void OnDestroy()
    {
        if(_blockedSockets.Count <= 0) { return; }

        foreach(TrapSocket socket in _blockedSockets)
        {
            socket.Unblock(this);
        }
    }
}
