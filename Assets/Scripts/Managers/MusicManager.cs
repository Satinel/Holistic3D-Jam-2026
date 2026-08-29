using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _buildMusic, _waveMusic, _winMusic, _loseMusic;

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
        if(_audioSource.isPlaying) { return; }

        _audioSource.clip = _buildMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    void LevelManager_OnWaveStarted()
    {
        _audioSource.clip = _waveMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    void LevelManager_OnWaveCompleted(int obj)
    {
        _audioSource.clip = _buildMusic;
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
