using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] float _destroyTime = 0.75f, _speed = 1f;
    [SerializeField] TextMeshProUGUI _text;

    void Update()
    {
        FaceCamera();
    }

    void FaceCamera()
    {
        transform.LookAt(Camera.main.transform);
        transform.Translate(_speed * Time.deltaTime * Vector3.up, Space.World);
    }

    public void SetUp(string newText, Color color)
    {
        _text.text = newText;
        _text.color = color;
        Destroy(gameObject, _destroyTime);
    }
}
