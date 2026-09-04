using UnityEngine;

[CreateAssetMenu(fileName = "CameraOptionsSO", menuName = "Scriptable Objects/CameraOptionsSO")]
public class CameraOptionsSO : ScriptableObject
{
    [field:SerializeField] public bool InvertY { get; private set; }
    [field:SerializeField] public bool IsFirstPerson { get; private set; }
    [field:SerializeField] public float SensitivityMultiplyer { get; private set; } = 1f;
    [field:SerializeField] public float FieldOfView { get; private set; } = 70f;
    [field:SerializeField] public float CameraSide { get; private set; } = 0.85f;

    public void SetInvertY(bool isInverted)
    {
        InvertY = isInverted;
    }

    public void SetFirstPerson(bool isFirstPerson)
    {
        IsFirstPerson = isFirstPerson;
    }

    public void SetSensitivity(float sensitivity)
    {
        SensitivityMultiplyer = sensitivity;
    }

    public void SetFOV(float newFOV)
    {
        FieldOfView = newFOV;
    }

    public void SetCamearSide(float sideValue)
    {
        CameraSide = Mathf.Clamp01(sideValue);
    }
}
