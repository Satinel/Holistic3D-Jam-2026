using UnityEngine;

interface ISapable
{
    public void Sap(float duration);
}

public class Sapable : MonoBehaviour, ISapable
{
    [SerializeField] Trap _sapableTrap;

    void Awake()
    {
        if(!_sapableTrap)
        {
            _sapableTrap = GetComponentInParent<Trap>();
        }

        if(!_sapableTrap)
        {
            gameObject.SetActive(false);
        }
    }

    public void Sap(float duration)
    {
        if(_sapableTrap is ISapable sapable)
        {
            sapable.Sap(duration);
        }
    }
}
