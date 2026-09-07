using UnityEngine;

public class SocketBlocker : MonoBehaviour
{
    TrapSocket _blockedSocket;

    void OnTriggerEnter(Collider other)
    {
        if(_blockedSocket) { return; }

        if(other.TryGetComponent(out TrapSocket socket))
        {
            _blockedSocket = socket;
            _blockedSocket.Block();
        }
    }

    void OnDestroy()
    {
        if(_blockedSocket)
        {
            _blockedSocket.Unblock();
        }
    }
}
