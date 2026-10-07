using UnityEngine;

public class SocketBlocker : MonoBehaviour
{
    [field:SerializeField] public Trap BlockingTrap { get; private set; }
    TrapSocket _blockedSocket;

    void OnTriggerEnter(Collider other)
    {
        if(_blockedSocket) { return; }

        if(other.TryGetComponent(out TrapSocket socket))
        {
            _blockedSocket = socket;
            _blockedSocket.Block(this);
        }
    }

    void OnDestroy()
    {
        if(_blockedSocket)
        {
            _blockedSocket.Unblock(this);
        }
    }
}
