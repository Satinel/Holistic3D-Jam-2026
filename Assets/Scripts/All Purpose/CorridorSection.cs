using UnityEngine;

public class CorridorSection : MonoBehaviour
{
    [SerializeField] GameObject[] _objectsToEnable;

    public void EnableObjects()
    {
        foreach(GameObject gObject in _objectsToEnable)
        {
            gObject.SetActive(true);
        }
    }

    public void DisableObjects()
    {
        foreach(GameObject gObject in _objectsToEnable)
        {
            gObject.SetActive(false);
        }
    }
}
