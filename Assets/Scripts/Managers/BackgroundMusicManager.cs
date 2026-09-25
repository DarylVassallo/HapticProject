using UnityEngine;

using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip firstSectionMusic;
    [SerializeField] private AudioClip secondSectionMusic;
    [SerializeField] private AudioClip thirdSectionMusic;
    [SerializeField] private AudioClip fourthSectionMusic;

    [SerializeField] private Transform endPoint;
    private bool _usingFourthSectionMusic;
    private Transform _pcPlayer;

    [SerializeField] private float _maxVolume;

    private void Awake()
    {
        _usingFourthSectionMusic = false;
    }

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerData;

        EventsManager.OnReachedSwitches += PlayFirstSectionMusic;
        EventsManager.OnPlaySecondSectionMusic += PlaySecondSectionMusic;
        EventsManager.OnPlayThirdSectionMusic += PlayThirdSectionMusic;
        EventsManager.OnPlayFourthSectionMusic += PlayFourthSectionMusic;

        EventsManager.OnPlayNarratorHaptic += LowerBackgroundMusic;
        EventsManager.OnStopNarratorHaptic += RaiseBackgroundMusic;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerData;

        EventsManager.OnReachedSwitches -= PlayFirstSectionMusic;
        EventsManager.OnPlaySecondSectionMusic -= PlaySecondSectionMusic;
        EventsManager.OnPlayThirdSectionMusic -= PlayThirdSectionMusic;
        EventsManager.OnPlayFourthSectionMusic -= PlayFourthSectionMusic;

        EventsManager.OnPlayNarratorHaptic -= LowerBackgroundMusic;
        EventsManager.OnStopNarratorHaptic -= RaiseBackgroundMusic;
    }

    private void GetPCPlayerData()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;

        // PlayFourthSectionMusic();
    }

    private void LowerBackgroundMusic()
    {
        StartCoroutine(ChangeMusicVolume(1f, audioSource.volume, 0.0015f));
    }

    private void RaiseBackgroundMusic()
    {
        StartCoroutine(ChangeMusicVolume(1f, audioSource.volume, _maxVolume));
    }

    IEnumerator ChangeMusicVolume(float _delay, float _currentVolume, float _requiredVolume)
    {
        float _elapsed = 0f;

        while(_elapsed < _delay)
        {
            _elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(_currentVolume, _requiredVolume, (_elapsed / _delay)); 
            yield return null;
        }

        audioSource.volume = _requiredVolume;
    }

    private void PlayFirstSectionMusic()
    {
        _maxVolume = 0.0025f;
        PlaySectionMusic(firstSectionMusic);
    }

    private void PlaySecondSectionMusic()
    {
        _maxVolume = 0.0025f;
        PlaySectionMusic(secondSectionMusic);
    }

    private void PlayThirdSectionMusic()
    {
        _maxVolume =0.0025f;
        PlaySectionMusic(thirdSectionMusic);
    }

    private void PlayFourthSectionMusic()
    {
        _usingFourthSectionMusic = true;
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;

        _maxVolume = 0.0025f;
        PlaySectionMusic(fourthSectionMusic);
    }
    
    private void PlaySectionMusic(AudioClip _music)
    {
        StartCoroutine(ChangeMusicClip(_music, 1f, audioSource.volume));
    }

    IEnumerator ChangeMusicClip(AudioClip _music, float _delay, float _currentVolume)
    {
        float _elapsed = 0f;
        while(_elapsed < _delay)
        {
            _elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(_currentVolume, 0f, (_elapsed / _delay)); 
            yield return null;
        }

        audioSource.volume = 0f;

        audioSource.Stop();
        audioSource.clip = _music;
        audioSource.loop = true;
        audioSource.Play();

        _elapsed = 0f;
        while(_elapsed < _delay)
        {
            _elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, _maxVolume, (_elapsed / _delay)); 
            yield return null;
        }

        audioSource.volume = _maxVolume;
    }

    private void Update()
    {
        if(!_usingFourthSectionMusic) return;

        float distRatio = 1f - (Vector3.Distance(_pcPlayer.position, endPoint.position) / 40f);
        if(distRatio < 0) distRatio = 0;
        audioSource.volume = Mathf.Lerp(0f, _maxVolume * 2, distRatio); 
    }
}
