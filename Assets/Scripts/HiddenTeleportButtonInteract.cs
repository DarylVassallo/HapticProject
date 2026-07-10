using UnityEngine;
using System;
using System.Collections.Generic;

using System.Collections;

using Unity.Netcode;

//This script controls is the button can be interacted with, and for what switch it is for
public class HiddenTeleportButtonInteract : NetworkBehaviour, IInteractable
{    
    public static event Action OnTriggerHiddenButton;
    public static event Action OnResetHiddenButton;
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;

    [SerializeField] private float destroyDelay;

    public static event Action<GameObject> OnRemoveHiddenObject;

    private MeshRenderer renderer;
    private RevealUnderLight revealUnderLight;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        renderer = GetComponent<MeshRenderer>();
        revealUnderLight = GetComponent<RevealUnderLight>();
    }

    private void OnEnable()
    {
        CheckpointManager.OnResetHiddenButtons += ResetHiddenButton;
    }

    private void OnDisable()
    {
        CheckpointManager.OnResetHiddenButtons -= ResetHiddenButton;
    }

    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            TriggerInteractionRpc();
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TriggerInteractionRpc()
    {
        OnTriggerHiddenButton?.Invoke();

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = buttonAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 

            // StartCoroutine(DestroyHiddenButton());

            OnRemoveHiddenObject?.Invoke(this.gameObject);
            renderer.enabled = false;
            DisableInteraction();
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

        if(_isInteractable)
        {
            OnRemoveHiddenObject?.Invoke(this.gameObject);
            renderer.enabled = false;
            DisableInteraction();
            // Destroy(this.gameObject);
        }
    }

    private void ResetHiddenButton()
    {
        if(!_isInteractable)
        {
            revealUnderLight.AddObject();
            OnResetHiddenButton?.Invoke();
            renderer.enabled = true;
            EnableInteraction();
        }
    }
}
