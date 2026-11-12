using UnityEngine;

public class Sapper : MonoBehaviour
{
    [SerializeField] float _sapDuration = 3.75f, _maxSize = 10, _scaleRate = 4.5f;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent(out ISapable sapable))
        {
            sapable.Sap(_sapDuration);
        }
    }

    void Update()
    {
        float size = Mathf.MoveTowards(transform.localScale.x, _maxSize, _scaleRate * Time.deltaTime);

        transform.localScale = Vector3.one * size;

        if(size >= _maxSize)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(float duration, float radius)
    {
        _sapDuration = duration;
        _maxSize = radius;
    }
}
