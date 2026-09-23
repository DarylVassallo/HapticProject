using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine.Audio;

//This script controls is the button can be interacted with, and for what switch it is for
public class ButtonInteract : NetworkBehaviour, IInteractable
{    
    private bool _isInteractable = true;    

    [SerializeField] private EventsManager.ShapeType shape;
    [SerializeField] private EventsManager.ButtonType button;

    [SerializeField] private AudioClip buttonAudio;

    private float pressedDistance = 0.1f;

    private Vector3 targetPosition;
    private Vector3 originalPosition;
    private Vector3 pushedPosition;

    // private float _buttonSpeed = 20f;

    // private bool _moveDown;
    // private bool _moveUp;

    private float _minDistance = 0.005f;

    private AudioSource _audioSource;
    // private bool _isPlaying;

    private Renderer _renderer;

    [SerializeField] private Material deactiveMaterial;
    [SerializeField] private Material inProgressMaterial;
    [SerializeField] private Material completeMaterial;
    private Material targetMaterial;

    [SerializeField] private bool isResetButton;

    private bool _activeButton;
    private bool _resetButton;

    private bool _isPermanentallyCorrect;

    void Awake()
    {
        // _isPlaying = false;

        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _audioSource.clip = buttonAudio;

        _renderer = this.gameObject.GetComponent<Renderer>();
        _renderer.material = deactiveMaterial;

        _activeButton = false;
        _resetButton = false;

        originalPosition = this.transform.position;

        //This line formed with ChatGPT
        pushedPosition =    this.transform.position + 
                            (   transform.forward * 
                                pressedDistance * 
                                this.transform.localScale.x
                            ); 
    }

    private void OnEnable()
    {
        EventsManager.OnResetButtons += ResetButtonRpc;
        EventsManager.OnFreezeCorrectButtons += FreezeButtonRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnResetButtons -= ResetButtonRpc;
        EventsManager.OnFreezeCorrectButtons -= FreezeButtonRpc;
    }

    //Override function from IInteractable
    public void TriggerInteraction()
    {
        if (_isInteractable && !_activeButton)
        {
            ActivateButtonRpc();
        }
    }


    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateButtonRpc()
    {        
        _audioSource.Stop();

        // _isPlaying = true;

        _activeButton = true;

        if(!isResetButton && IsOwner) EventsManager.TriggerButton(shape, button);

        targetPosition = originalPosition;
        targetMaterial = inProgressMaterial;
        StartCoroutine(ButtonMove(4f, true));

        // _moveDown = true;
        
        _audioSource.Play();
    }

    public void DeactivateButton()
    {
        _activeButton = false;
        _resetButton = true;
        targetPosition = pushedPosition; 
        targetMaterial = deactiveMaterial;
        StartCoroutine(ButtonMove(4f, false));
        
        // _moveUp = true;
    }

    //Override function from IInteractable
    public void EnableInteraction()
    {
        _isInteractable = true;
    }

    //Override function from IInteractable
    public void DisableInteraction()
    {
        _isInteractable = false;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void TrueResetButtonRpc()
    {
        _isPermanentallyCorrect = false;
        if(!_activeButton) return;
        DeactivateButton();
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ResetButtonRpc()
    {
        if(!_activeButton || _isPermanentallyCorrect) return;
        DeactivateButton();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void FreezeButtonRpc(EventsManager.ShapeType _shape)
    {
        if(!_activeButton) return;

        if(_shape == shape)
        {
            _isPermanentallyCorrect = true;
        }
        else
        {
            DeactivateButton();
        }
    }

    IEnumerator ButtonMove(float _delay, bool _canReset)
    {
        float elapsed = 0f;

        if(isResetButton && _canReset)
        {
            EventsManager.ActivateReset();
        }

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            this.transform.position = Vector3.Lerp(
                this.transform.position,
                targetPosition,
                elapsed / _delay
            );

            _renderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    _renderer.material.GetColor("_BaseColor"),
                    targetMaterial.GetColor("_BaseColor"),
                    elapsed / _delay
                )
            );

            if(_resetButton && _canReset) elapsed = _delay;

            yield return null;
        }

        if(!(_resetButton && _canReset))
        {
            this.transform.position = targetPosition;
            _renderer.material = targetMaterial;
        }

        if(_resetButton && _canReset)
        {
            DeactivateButton();
        }

        if(!_canReset) _resetButton = false;
    }
}
