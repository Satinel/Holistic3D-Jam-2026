using System.Collections.Generic;
using UnityEngine;

public class TrapPreview : MonoBehaviour
{
    [SerializeField] MeshRenderer[] _renderers;
    [SerializeField] GameObject _rangeIndicator;

    public void SetMaterials(Material material)
    {
        List<Material> materials = new() { material };
        foreach(MeshRenderer renderer in _renderers)
        {
            renderer.SetMaterials(materials);
        }
    }

    public void ShowRange(bool shouldShow)
    {
        _rangeIndicator.SetActive(shouldShow);
    }
}
