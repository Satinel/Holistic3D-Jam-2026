using UnityEngine;

public class CursorManager : MonoBehaviour
{

    void Awake()
    {
        HideCursor();
    }

    void OnEnable()
    {
        VolumeControl.OnAudioCanvasToggled += SetShouldShowCursor;
    }

    void OnDisable()
    {
        VolumeControl.OnAudioCanvasToggled -= SetShouldShowCursor;
    }

    void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetShouldShowCursor(bool shouldShow)
    {
        if(shouldShow)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }
}
