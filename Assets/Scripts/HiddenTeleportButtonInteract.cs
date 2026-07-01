using UnityEngine;
using System;
using System.Collections.Generic;

using System.Collections;

//This script controls is the button can be interacted with, and for what switch it is for
public class HiddenTeleportButtonInteract : MonoBehaviour, IInteractable
{    
    public static event Action OnTriggerHiddenButton;
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;

    [SerializeField] private float destroyDelay;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            OnTriggerHiddenButton?.Invoke();

            if(!_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = buttonAudio;
                _audioSource.Play();
                _audioSource.enabled = true; 

                StartCoroutine(DestroyHiddenButton());
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

    IEnumerator DestroyHiddenButton()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(this.gameObject);
    }
}
