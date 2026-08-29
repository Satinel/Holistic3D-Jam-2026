using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    float _currentTimeScale = 1f;
    bool _levelComplete;

    void OnEnable()
    {
        VolumeControl.OnAudioCanvasToggled += VolumeControl_OnAudioCanvasToggled;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelLoaded += LevelManager_OnLevelLoaded;
    }

    void OnDisable()
    {
        VolumeControl.OnAudioCanvasToggled -= VolumeControl_OnAudioCanvasToggled;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelLoaded -= LevelManager_OnLevelLoaded;
    }

    void VolumeControl_OnAudioCanvasToggled(bool isEnabled)
    {
        if(isEnabled)
        {
            Time.timeScale = 0;
        }
        else if(!_levelComplete)
        {
            Time.timeScale = _currentTimeScale;
        }
    }

    void LevelManager_OnLevelCompleted()
    {
        Time.timeScale = 0;
        _levelComplete = true;
    }

    void LevelManager_OnLevelLoaded()
    {
        Time.timeScale = _currentTimeScale;
    }
}
