using System.Collections.Generic;
using UnityEngine;

public class TrapPreview : MonoBehaviour
{
    [SerializeField] MeshRenderer[] _renderers;
    [SerializeField] GameObject _rangeIndicator;

    static readonly int COLOR_ID = Shader.PropertyToID("_BaseColor");

    public void SetMaterials(Color color)
    {
        MaterialPropertyBlock mpb = new();
        foreach(MeshRenderer renderer in _renderers)
        {
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor(COLOR_ID, color);
            renderer.SetPropertyBlock(mpb);
        }
    }

    public void ShowRange(bool shouldShow)
    {
        _rangeIndicator.SetActive(shouldShow);
    }
}
