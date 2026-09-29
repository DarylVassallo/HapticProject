using UnityEngine;

using System.Collections;
using Unity.Netcode;

public class BackgroundMusicManager : NetworkBehaviour
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
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerDataRpc;

        EventsManager.OnReachedSwitches += PlayFirstSectionMusicRpc;
        EventsManager.OnPlaySecondSectionMusic += PlaySecondSectionMusicRpc;
        EventsManager.OnPlayThirdSectionMusic += PlayThirdSectionMusicRpc;
        EventsManager.OnPlayFourthSectionMusic += PlayFourthSectionMusicRpc;

        EventsManager.OnPlayNarratorHaptic += LowerBackgroundMusicRpc;
        EventsManager.OnStopNarratorHaptic += RaiseBackgroundMusicRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerDataRpc;

        EventsManager.OnReachedSwitches -= PlayFirstSectionMusicRpc;
        EventsManager.OnPlaySecondSectionMusic -= PlaySecondSectionMusicRpc;
        EventsManager.OnPlayThirdSectionMusic -= PlayThirdSectionMusicRpc;
        EventsManager.OnPlayFourthSectionMusic -= PlayFourthSectionMusicRpc;

        EventsManager.OnPlayNarratorHaptic -= LowerBackgroundMusicRpc;
        EventsManager.OnStopNarratorHaptic -= RaiseBackgroundMusicRpc;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetPCPlayerDataRpc()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;

        // PlayFourthSectionMusicRpc();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void LowerBackgroundMusicRpc()
    {
        StartCoroutine(ChangeMusicVolume(1f, audioSource.volume, 0.0015f));
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void RaiseBackgroundMusicRpc()
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

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayFirstSectionMusicRpc()
    {
        _maxVolume = 0.0025f;
        PlaySectionMusic(firstSectionMusic);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlaySecondSectionMusicRpc()
    {
        _maxVolume = 0.0025f;
        PlaySectionMusic(secondSectionMusic);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayThirdSectionMusicRpc()
    {
        _maxVolume =0.0025f;
        PlaySectionMusic(thirdSectionMusic);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayFourthSectionMusicRpc()
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
