using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _buildMusic, _waveMusic, _winMusic, _loseMusic, _bossMusic;
    [SerializeField] float _clipStartPoint = 0f;

    int _clipPausePoint = 0;
    bool _isPlayingBossMusic;

    void Awake()
    {
        LevelManager.OnLevelStarted += LevelManager_OnLevelStarted;
        LevelManager.OnWaveStarted += LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted += LevelManager_OnSceneChangeStarted;
        Enemy.OnBossSpawned += Enemy_OnBossSpawned;
    }

    void OnDestroy()
    {
        LevelManager.OnLevelStarted -= LevelManager_OnLevelStarted;
        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed -= LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted -= LevelManager_OnSceneChangeStarted;
        Enemy.OnBossSpawned -= Enemy_OnBossSpawned;
    }

    void LevelManager_OnLevelStarted()
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

    void Enemy_OnBossSpawned()
    {
        if(_isPlayingBossMusic) { return; }

        _isPlayingBossMusic = true;
        _audioSource.Stop();
        _audioSource.clip = _bossMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }
}
