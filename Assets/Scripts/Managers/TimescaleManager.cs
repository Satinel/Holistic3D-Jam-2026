using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    float _currentTimeScale = 1f;

    void OnEnable()
    {
        VolumeControl.OnAudioCanvasToggled += VolumeControl_OnAudioCanvasToggled;
    }

    void OnDisable()
    {
        VolumeControl.OnAudioCanvasToggled -= VolumeControl_OnAudioCanvasToggled;
    }

    private void VolumeControl_OnAudioCanvasToggled(bool isEnabled)
    {
        if(isEnabled)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = _currentTimeScale;
        }
    }
}
