using UnityEngine;

using Unity.Netcode;

public class NarratorManager : NetworkBehaviour
{
    private AudioSource _audioSource;
    private bool _isPlaying;

    private ulong _pcPlayerID;
    private ulong _vrPlayerID;

    [System.Serializable]
    private struct NarratorSection
    {
        public string sectionName;
        public NarratorAudio[] narratorAudio;
    }
    
    [System.Serializable]
    private struct NarratorAudio
    {
        public string audioName;
        public AudioClip audioClip;
    }
    [SerializeField] private NarratorSection[] narratorLines;

    private void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _pcPlayerID = unchecked((ulong)-1);
        _vrPlayerID = unchecked((ulong)-1);
    }

    private void OnEnable()
    {
        EventsManager.OnTriggerNarratorAudio += TriggerNarratorAudioRpc;
        EventsManager.OnSetPCPlayerID += SetPCPlayerIDRpc;
        EventsManager.OnSetVRPlayerID += SetVRPlayerIDRpc;

        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerNarratorAudio -= TriggerNarratorAudioRpc;
        EventsManager.OnSetPCPlayerID -= SetPCPlayerIDRpc;
        EventsManager.OnSetVRPlayerID -= SetVRPlayerIDRpc;

        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TogglePauseManagerAudioRpc(bool _toggle)
    {
        if(_toggle)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetPCPlayerIDRpc(ulong _newID)
    {
        _pcPlayerID = _newID;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetVRPlayerIDRpc(ulong _newID)
    {
        _vrPlayerID = _newID;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TriggerNarratorAudioRpc(string _currentSection, string _currentAudio, bool _isVRPlayer)
    {
        if( _vrPlayerID == unchecked((ulong)-1) && _isVRPlayer || 
            _pcPlayerID == unchecked((ulong)-1) && !_isVRPlayer ||
            !_isVRPlayer && NetworkManager.Singleton.LocalClientId == _vrPlayerID ||
            _isVRPlayer && NetworkManager.Singleton.LocalClientId == _pcPlayerID) return;

        for(int i = 0; i < narratorLines.Length; i++)
        {
            if(narratorLines[i].sectionName == _currentSection)
            {
                for(int j = 0; j < narratorLines[i].narratorAudio.Length; j++)
                {
                    if(narratorLines[i].narratorAudio[j].audioName == _currentAudio)
                    {
                        _audioSource.Stop();
                        _audioSource.clip = narratorLines[i].narratorAudio[j].audioClip;
                        _audioSource.Play();
                        _isPlaying = true;
                        _audioSource.enabled = true; 

                        return;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if(!_isPlaying) return;

        if(!_audioSource.isPlaying)
        {
            _isPlaying = false;
            EventsManager.NarratorStopped();
        }
    }
}
