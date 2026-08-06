using UnityEngine;
using System;
using System.Collections.Generic;

using System.Collections;

using Unity.Netcode;

//This script controls is the button can be interacted with, and for what switch it is for
public class CollectableInteract : NetworkBehaviour, IInteractable
{    
    public static event Action OnTriggerHiddenButton;
    public static event Action OnResetHiddenButton;
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;

    [SerializeField] private float destroyDelay;

    public static event Action<GameObject> OnRemoveHiddenObject;

    private MeshRenderer renderer;
    [SerializeField] private MeshRenderer imageRenderer;

    public static event Action<GameObject, bool, bool, bool, bool> OnAddNewHiddenObject;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        renderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        CheckpointManager.OnResetHiddenButtons += ResetHiddenButton;
    }

    private void OnDisable()
    {
        CheckpointManager.OnResetHiddenButtons -= ResetHiddenButton;
    }

    //Triggers interaction, if the PCPlayer selects the collectable
    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            TriggerInteractionRpc();
        }
    }

    //If interacted with, the collectable is hidden
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

            OnRemoveHiddenObject?.Invoke(this.gameObject);
            
            ToggleRenderer(false);
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

    //Resets the collectable, to make it interactable and visible again
    private void ResetHiddenButton()
    {
        if(!_isInteractable)
        {
            OnAddNewHiddenObject?.Invoke(this.gameObject, true, false, true, false);
            OnResetHiddenButton?.Invoke();

            ToggleRenderer(true);
            EnableInteraction();
        }
    }

    //Toggle the collectable's rendering, to hid or reset it
    private void ToggleRenderer(bool toggle)
    {
        renderer.enabled = toggle;
        foreach (Transform child in this.transform)
        {
            child.GetComponent<MeshRenderer>().enabled = toggle;
        }

        imageRenderer.enabled = toggle;
    }
}
