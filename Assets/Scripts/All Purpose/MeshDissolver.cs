using UnityEngine;

public class MeshDissolver : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] Shader _dissolveShader;
    [SerializeField] [ColorUsage(true, true)] Color _color = new(4f, 1.4f, 0.3f, 1f);
    [SerializeField] float _dissolveRate = 0.25f;
    [SerializeField] Health _health;

    bool _isDissolving;
    float _dissolveAmount = 0;
    Material _material;

    const string DISSOLVE_STRING = "_Dissolve", COLOR_STRING = "_EdgeColor";

    void Awake()
    {
        if(_health)
        {
            _health.OnDeath += StartDissolve;
        }
    }

    void OnDestroy()
    {
        if(_health)
        {
            _health.OnDeath -= StartDissolve;
        }
    }

    void Update()
    {
        if(_isDissolving && _dissolveAmount < 1f)
        {
            _dissolveAmount = Mathf.Min(_dissolveAmount + (Time.deltaTime * _dissolveRate), 1f);

            _material.SetFloat(DISSOLVE_STRING, _dissolveAmount);
        }
    }

    void StartDissolve()
    {
        if(_isDissolving) { return; }
        if(!_skinnedMeshRenderer || !_dissolveShader) { return; }

        _material = _skinnedMeshRenderer.material;
        _material.shader = _dissolveShader;
        _material.SetColor(COLOR_STRING, _color);

        foreach(SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.sharedMaterial = _material;
        }

        _isDissolving = true;
    }
}
