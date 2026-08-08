using UnityEngine;
using System;
using System.Collections.Generic;

using System.Collections;

using Unity.Netcode;

//This script controls is the button can be interacted with, and for what switch it is for
public class CollectableInteract : NetworkBehaviour, IInteractable
{    
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;

    [SerializeField] private float destroyDelay;

    private MeshRenderer rend;
    [SerializeField] private MeshRenderer imageRenderer;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        rend = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        EventsManager.OnResetHiddenButtons += ResetHiddenButton;
    }

    private void OnDisable()
    {
        EventsManager.OnResetHiddenButtons -= ResetHiddenButton;
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
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TriggerInteractionRpc()
    {
        EventsManager.TriggerHiddenButton();

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = buttonAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 

            EventsManager.RemoveHiddenObject(this.gameObject);
            
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
            EventsManager.AddNewHiddenObject(this.gameObject, true, false, true, false);

            ToggleRenderer(true);
            EnableInteraction();
        }
    }

    //Toggle the collectable's rendering, to hid or reset it
    private void ToggleRenderer(bool toggle)
    {
        rend.enabled = toggle;
        foreach (Transform child in this.transform)
        {
            child.GetComponent<MeshRenderer>().enabled = toggle;
        }

        imageRenderer.enabled = toggle;
    }
}
