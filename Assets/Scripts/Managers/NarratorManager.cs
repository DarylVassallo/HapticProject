using UnityEngine;

using Unity.Netcode;

using Interhaptics;
using Interhaptics.Utils;

public class NarratorManager : NetworkBehaviour
{
    private AudioSource _audioSource;

    private ulong _pcPlayerID;
    private ulong _vrPlayerID;

    private bool _isEnglish;
    private bool _isFrench;

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
        public AudioClip englishAudioClip;
        public AudioHapticSource englishHapticSource;
        public AudioClip frenchAudioClip;
        public AudioHapticSource frenchHapticSource;
    }
    [SerializeField] private NarratorSection[] narratorLines;

    private void Awake()
    {
        UseEnglishNarrator();

        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _pcPlayerID = unchecked((ulong)-1);
        _vrPlayerID = unchecked((ulong)-1);

        for(int i = 0; i < narratorLines.Length; i++)
        {
            for(int j = 0; j < narratorLines[i].narratorAudio.Length; j++)
            {
                if(narratorLines[i].narratorAudio[j].englishHapticSource != null)   narratorLines[i].narratorAudio[j].englishHapticSource.enabled = false;
                if(narratorLines[i].narratorAudio[j].frenchHapticSource != null)   narratorLines[i].narratorAudio[j].frenchHapticSource.enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        EventsManager.OnTriggerNarratorAudio += TriggerNarratorAudioRpc;
        EventsManager.OnSetPCPlayerID += SetPCPlayerIDRpc;
        EventsManager.OnSetVRPlayerID += SetVRPlayerIDRpc;

        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;

        EventsManager.OnUseEnglishNarrator += UseEnglishNarrator;
        EventsManager.OnUseFrenchNarrator += UseFrenchNarrator;
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerNarratorAudio -= TriggerNarratorAudioRpc;
        EventsManager.OnSetPCPlayerID -= SetPCPlayerIDRpc;
        EventsManager.OnSetVRPlayerID -= SetVRPlayerIDRpc;

        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;

        EventsManager.OnUseEnglishNarrator -= UseEnglishNarrator;
        EventsManager.OnUseFrenchNarrator -= UseFrenchNarrator;
    }

    private void UseEnglishNarrator()
    {
        _isEnglish = true;
        _isFrench = false;
    }

    private void UseFrenchNarrator()
    {
       _isFrench = true;
       _isEnglish = false;
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
    public void TriggerNarratorAudioRpc(string _currentSection, string _currentAudio, bool _isVRPlayer, int _lookAt, bool _stay)
    {
        // if( _vrPlayerID == unchecked((ulong)-1) && _isVRPlayer || 
        //     _pcPlayerID == unchecked((ulong)-1) && !_isVRPlayer ||
        //     !_isVRPlayer && NetworkManager.Singleton.LocalClientId == _vrPlayerID ||
        //     _isVRPlayer && NetworkManager.Singleton.LocalClientId == _pcPlayerID) return;

        Debug.Log("TriggerNarratorAudioRpc: _currentSection: " + _currentSection + " : _currentAudio : " + _currentAudio + " : _isVRPlayer : " + _isVRPlayer + " : _lookAt : " + _lookAt + " : _stay : " + _stay);

        if(_isVRPlayer)
        {
            EventsManager.LookAtPlayer(0, _lookAt, _stay);
        }
        else
        {
            EventsManager.LookAtPlayer(1, _lookAt, _stay);
        }

        for(int i = 0; i < narratorLines.Length; i++)
        {
            if(narratorLines[i].sectionName == _currentSection)
            {
                for(int j = 0; j < narratorLines[i].narratorAudio.Length; j++)
                {
                    if(narratorLines[i].narratorAudio[j].audioName == _currentAudio)
                    {                        
                        if(_isEnglish) EventsManager.NarratorSays(narratorLines[i].narratorAudio[j].englishAudioClip, narratorLines[i].narratorAudio[j].englishHapticSource);
                        if(_isFrench) EventsManager.NarratorSays(narratorLines[i].narratorAudio[j].frenchAudioClip, narratorLines[i].narratorAudio[j].frenchHapticSource);

                        return;
                    }
                }
            }
        }
    }
}
