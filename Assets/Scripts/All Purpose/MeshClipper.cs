using UnityEngine;

public class MeshClipper : MonoBehaviour
{
    [SerializeField][Range(0.5f, 5f)] float _clipRate = 2f;
    [SerializeField] GameObject[] _baseModels = System.Array.Empty<GameObject>(), _clippedModels = System.Array.Empty<GameObject>();

    MeshRenderer[] _renderers = System.Array.Empty<MeshRenderer>();
    float _clipAmount = 0;
    MaterialPropertyBlock[] _matPropBlocks;
    bool _hasFinished;

    const string CLIP_STRING = "_ClipAmount";

    void Awake()
    {
        if(_clippedModels == null || _clippedModels.Length == 0) { return; }

        foreach(GameObject gameObj in _baseModels)
        {
            gameObj.SetActive(false);
        }

        foreach(GameObject gameObj in _clippedModels)
        {
            gameObj.SetActive(true);
        }

        _renderers = new MeshRenderer[_clippedModels.Length];

        for(int i = 0; i < _clippedModels.Length; i++)
        {
            _renderers[i] = _clippedModels[i].GetComponentInChildren<MeshRenderer>();
        }

        if(_renderers.Length <= 0) { return; }

        _matPropBlocks = new MaterialPropertyBlock[_renderers.Length];

        for(int i = 0; i < _renderers.Length; i++)
        {
            _matPropBlocks[i] = new();
            _renderers[i].GetPropertyBlock(_matPropBlocks[i]);
        }
    }

    void Update()
    {
        if(_hasFinished) { return; }
        if(_renderers.Length <= 0) { return; }

        if(_clipAmount < 1)
        {
            _clipAmount = Mathf.Min(_clipAmount + (Time.deltaTime * _clipRate), 1f);

            for(int i = 0; i < _renderers.Length; i++)
            {
                _matPropBlocks[i].SetFloat(CLIP_STRING, _clipAmount);
                _renderers[i].SetPropertyBlock(_matPropBlocks[i]);
            }

        }
        else
        {
            _hasFinished = true;

            foreach(GameObject gameObj in _baseModels)
            {
                gameObj.SetActive(true);
            }

            foreach(GameObject gameObj in _clippedModels)
            {
                gameObj.SetActive(false);
            }

            enabled = false;
        }
    }
}
