using UnityEngine;

using Unity.Netcode;

public class NarratorManager : NetworkBehaviour
{
    private AudioSource _audioSource;
    
    [System.Serializable]
    private struct NarratorAudio
    {
        public string section;
        public AudioClip[] audio;
    }
    [SerializeField] private NarratorAudio[] narratorAudio;

    private void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        EventsManager.OnTriggerNarratorAudio += TriggerNarratorAudioRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerNarratorAudio -= TriggerNarratorAudioRpc;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void TriggerNarratorAudioRpc(string _currentSection, int _audioNum, bool _isVRPlayer)
    {
        Debug.Log("TriggerNarratorAudio IsOwner: " + IsOwner);
        if(_isVRPlayer && !IsOwner || !_isVRPlayer && IsOwner) return;
        Debug.Log("TriggerNarratorAudio Playing");

        for(int i = 0; i < narratorAudio.Length; i++)
        {
            if(narratorAudio[i].section == _currentSection)
            {
                _audioSource.Stop();
                _audioSource.clip = narratorAudio[i].audio[_audioNum];
                _audioSource.Play();
                _audioSource.enabled = true; 
            }
        }
    }
}
