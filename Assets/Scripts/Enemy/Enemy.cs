using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject _model, _radgollPrefab;

    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;

        if(_timer > 5f)
        {
            _model.SetActive(false);
            Instantiate(_radgollPrefab, _model.transform.position, _model.transform.rotation, transform);
            enabled = false;
        }
    }
}
