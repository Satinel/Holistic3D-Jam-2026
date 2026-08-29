using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _buildMusic, _waveMusic, _winMusic, _loseMusic;
    [SerializeField] float _clipStartPoint = 0f;

    int _clipPausePoint = 0;

    void Awake()
    {
        LevelManager.OnLevelLoaded += LevelManager_OnLevelLoaded;
        LevelManager.OnWaveStarted += LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted += LevelManager_OnSceneChangeStarted;
    }

    void OnDestroy()
    {
        LevelManager.OnLevelLoaded -= LevelManager_OnLevelLoaded;
        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed -= LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted -= LevelManager_OnSceneChangeStarted;
    }

    void LevelManager_OnLevelLoaded()
    {
        _audioSource.clip = _buildMusic;
        _audioSource.loop = true;
        _audioSource.time = _clipStartPoint;
        _audioSource.Play();
    }

    void LevelManager_OnWaveStarted()
    {
        if(_audioSource.isPlaying)
        {
            _clipPausePoint = _audioSource.timeSamples;

        }
        _audioSource.clip = _waveMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    void LevelManager_OnWaveCompleted(int obj, int _)
    {
        _audioSource.Stop();
        _audioSource.clip = _buildMusic;
        _audioSource.timeSamples = _clipPausePoint;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    void LevelManager_OnLevelCompleted()
    {
        _audioSource.clip = _winMusic;
        _audioSource.loop = false;
        _audioSource.Play();
    }

    void LevelManager_OnLevelFailed()
    {
        _audioSource.clip = _loseMusic;
        _audioSource.loop = false;
        _audioSource.Play();
    }

    void LevelManager_OnSceneChangeStarted()
    {
        _audioSource.Stop();
    }
}
