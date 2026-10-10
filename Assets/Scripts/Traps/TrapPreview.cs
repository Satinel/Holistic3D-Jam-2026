using UnityEngine;

public class TrapPreview : MonoBehaviour
{
    [field:SerializeField] public Transform[] SocketPoints { get; private set; }
    [SerializeField] MeshRenderer[] _renderers;
    [SerializeField] GameObject _rangeIndicator;
    [SerializeField] Color _buyColor = Color.green, _poorColor = Color.red;
    [SerializeField] Quaternion[] _rotations;

    int _rotationIndex;

    static readonly int EMISSION_COLOR_ID = Shader.PropertyToID("_EmissionColor");

    public Quaternion GetRotation(Quaternion socketRotation)
    {
        transform.rotation = socketRotation;
        if(_rotationIndex > 0)
        {
            transform.Rotate(_rotations[_rotationIndex].eulerAngles);
        }
        return transform.rotation;
    }

    public void SetRotation()
    {
        _rotationIndex++;
        _rotationIndex %= _rotations.Length;
    }

    public void SetMaterials(bool canBuy)
    {
        _rangeIndicator.SetActive(canBuy);
        MaterialPropertyBlock mpb = new();
        foreach(MeshRenderer renderer in _renderers)
        {
            renderer.GetPropertyBlock(mpb);
            Color color = canBuy ? _buyColor : _poorColor;
            mpb.SetColor(EMISSION_COLOR_ID, color);
            renderer.SetPropertyBlock(mpb);
        }
    }
}
