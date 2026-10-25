using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _buildMusic, _winMusic, _loseMusic;
    [SerializeField] AudioClip[] _waveSongs;
    [SerializeField] float _clipStartPoint = 0f;

    int _clipPausePoint = 0;
    AudioClip _waveMusic;

    void Awake()
    {
        LevelManager.OnLevelStarted += LevelManager_OnLevelStarted;
        LevelManager.OnWaveStarted += LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted += LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted += LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed += LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted += LevelManager_OnSceneChangeStarted;
        TimescaleManager.OnTimeScaleChanged += TimescaleManager_OnTimeScaleChanged;
    }

    void OnDestroy()
    {
        LevelManager.OnLevelStarted -= LevelManager_OnLevelStarted;
        LevelManager.OnWaveStarted -= LevelManager_OnWaveStarted;
        LevelManager.OnWaveCompleted -= LevelManager_OnWaveCompleted;
        LevelManager.OnLevelCompleted -= LevelManager_OnLevelCompleted;
        LevelManager.OnLevelFailed -= LevelManager_OnLevelFailed;
        LevelManager.OnSceneChangeStarted -= LevelManager_OnSceneChangeStarted;
        TimescaleManager.OnTimeScaleChanged -= TimescaleManager_OnTimeScaleChanged;
    }

    void Start()
    {
        _waveMusic = _waveSongs[0];
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

    void LevelManager_OnWaveCompleted(int waveIndex, int waveRewards)
    {
        _audioSource.Stop();
        _audioSource.clip = _buildMusic;
        _audioSource.timeSamples = _clipPausePoint;
        _audioSource.loop = true;
        _audioSource.Play();

        if(waveIndex < _waveSongs.Length - 1)
        {
            _waveMusic = _waveSongs[waveIndex];
        }
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

    void TimescaleManager_OnTimeScaleChanged(float speed)
    {
        _audioSource.pitch = speed > 0 ? speed : 1f;
    }
}
