using UnityEngine;

interface ISapable
{
    public void Sap(float duration);
}

public class Sapable : MonoBehaviour, ISapable
{
    [SerializeField] Trap _sapableTrap;
    [SerializeField] GameObject _model;

    float _timer;

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

    void Update()
    {
        if(_timer > 0)
        {
            _timer -= Time.deltaTime;

            if(_timer <= 0)
            {
                _model.SetActive(false);
                _timer = 0;
            }
        }
    }

    public void Sap(float duration)
    {
        if(_sapableTrap is ISapable sapable)
        {
            sapable.Sap(duration);
            if(_model)
            {
                _timer += duration;
                _model.SetActive(true);
            }
        }
    }
}
