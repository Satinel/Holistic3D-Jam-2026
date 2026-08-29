using System.Collections;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] CorridorSection[] _sections;
    [SerializeField] float _wait = 0.1f;
    WaitForSeconds _waitForSeconds;


    void Start()
    {
        _waitForSeconds = new WaitForSeconds(_wait);
    }

    public void DeactivatePassage()
    {
        foreach(CorridorSection section in _sections)
        {
            section.DisableObjects();
        }
    }

    public void ActivatePath()
    {
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        foreach(CorridorSection section in _sections)
        {
            section.EnableObjects();
            yield return _waitForSeconds;
        }
    }
}
