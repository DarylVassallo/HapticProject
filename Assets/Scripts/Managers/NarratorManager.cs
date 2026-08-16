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
        Debug.Log("NarratorManager IsOwner: " + IsOwner);
    }

    private void OnEnable()
    {
        EventsManager.OnTriggerNarratorAudio += TriggerNarratorAudio;
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerNarratorAudio -= TriggerNarratorAudio;
    }

    private void TriggerNarratorAudio(string _currentSection, int _audioNum)
    {
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
