using UnityEngine;

public class TeleportButton : MonoBehaviour
{
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    //Triggers interaction, if the PCPlayer selects the collectable
    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            EventsManager.TriggerTeleportButton(this.transform.parent.gameObject);

            if(!_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = buttonAudio;
                _audioSource.Play();
                _audioSource.enabled = true; 
            }
        }
    }

    public void EnableInteraction()
    {
        _isInteractable = true;
    }

    public void DisableInteraction()
    {
        _isInteractable = false;
    }
}
