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
    [SerializeField] private Material wrongMaterial;
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
        pushedPosition =    this.transform.position - 
                            (   transform.forward * 
                                pressedDistance * 
                                this.transform.localScale.x
                            ); 
    }

    private void OnEnable()
    {
        EventsManager.OnResetButtons += ResetButtonRpc;
        EventsManager.OnTrueResetButtons += TrueResetButtonRpc;
        EventsManager.OnFreezeCorrectButtons += FreezeButtonRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnResetButtons -= ResetButtonRpc;
        EventsManager.OnTrueResetButtons -= TrueResetButtonRpc;
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

        _activeButton = true;

        if(!isResetButton && IsOwner) EventsManager.TriggerButton(shape, button);

        targetPosition = pushedPosition;
        targetMaterial = inProgressMaterial;
        StartCoroutine(ButtonMove(0.25f, true, false));
        
        _audioSource.Play();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeactivateButtonRpc(bool _isWrong)
    {
        _activeButton = false;
        _resetButton = true;
        targetPosition = originalPosition; 

        if(_isWrong)
        {
            targetMaterial = wrongMaterial;
        }
        else
        {
            targetMaterial = deactiveMaterial;
        }

        StartCoroutine(ButtonMove(0.25f, false, _isWrong));
        
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
        if(IsOwner) DeactivateButtonRpc(true);
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ResetButtonRpc()
    {
        if(!_activeButton || _isPermanentallyCorrect) return;
        if(IsOwner) DeactivateButtonRpc(true);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void FreezeButtonRpc(EventsManager.ShapeType _shape)
    {
        if(!_activeButton) return;

        if(_shape == shape)
        {
            _isPermanentallyCorrect = true;
            StartCoroutine(ButtonCorrect(0.25f));
        }
        else
        {
            if(IsOwner) DeactivateButtonRpc(false);
        }
    }

    IEnumerator ButtonCorrect(float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            _renderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    _renderer.material.GetColor("_BaseColor"),
                    completeMaterial.GetColor("_BaseColor"),
                    elapsed / _delay
                )
            );

            yield return null;
        }
    }

    IEnumerator ButtonMove(float _delay, bool _canReset, bool _isWrong)
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
            DeactivateButtonRpc(false);
        }

        if(!_canReset) _resetButton = false;

        if(_isWrong)
        {
            elapsed = 0f;
            targetMaterial = deactiveMaterial;

            while(elapsed < (_delay / 2f))
            {
                elapsed += Time.deltaTime;

                _renderer.material.SetColor(
                    "_BaseColor",
                    Color.Lerp(
                        _renderer.material.GetColor("_BaseColor"),
                        targetMaterial.GetColor("_BaseColor"),
                        elapsed / (_delay / 2f)
                    )
                );

                yield return null;
            }
        }
    }
}
