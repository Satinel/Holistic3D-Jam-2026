using UnityEngine;

[CreateAssetMenu(fileName = "CameraOptionsSO", menuName = "Scriptable Objects/CameraOptionsSO")]
public class CameraOptionsSO : ScriptableObject
{
    [field:SerializeField] public bool InvertY { get; private set; }
    [field:SerializeField] public float SensitivityMultiplyer { get; private set; } = 1f;

    public void SetInvertY(bool isInverted)
    {
        InvertY = isInverted;
    }

    public void SetSensitivity(float sensitivity)
    {
        SensitivityMultiplyer = sensitivity;
    }
}
