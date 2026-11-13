using UnityEngine;

public class Sapper : MonoBehaviour
{
    [SerializeField] float _sapDuration = 3.75f, _maxSize = 10, _scaleRate = 4.5f;

    Transform _fakeParent;

    void OnTriggerEnter(Collider collider)
    {
        if(collider.TryGetComponent(out ISapable sapable))
        {
            sapable.Sap(_sapDuration);
        }
    }

    void Update()
    {
        if(_fakeParent)
        {
            transform.position = _fakeParent.position;  // Note : Simply parenting this to '_fakeParent' allows this to be crushed/squished/etc.
        }

        float size = Mathf.MoveTowards(transform.localScale.x, _maxSize, _scaleRate * Time.deltaTime);

        transform.localScale = Vector3.one * size;

        if(size >= _maxSize)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(float duration, float radius, Transform parent)
    {
        _sapDuration = duration;
        _maxSize = radius;
        _fakeParent = parent;
    }
}
