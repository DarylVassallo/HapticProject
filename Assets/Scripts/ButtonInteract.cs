using UnityEngine;
using System;
using System.Collections.Generic;

using Unity.Netcode;

//This script controls is the button can be interacted with, and for what switch it is for
public class ButtonInteract : NetworkBehaviour, IInteractable
{    
    private bool _isInteractable = true;    

    [SerializeField] private EventsManager.ShapeType shape;
    [SerializeField] private EventsManager.ButtonType button;

    private float pressedDistance = 0.1f;
    private bool _isFullyPressed = false;

    private Vector3 targetPosition;
    private Vector3 originalPosition;
    private Vector3 pushedPosition;

    private float _buttonSpeed = 20f;

    private bool _moveDown;
    private bool _moveUp;

    private float _minDistance = 0.005f;

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;
    private Renderer _renderer;

    [SerializeField] private Material deactiveMaterial;
    [SerializeField] private Material inProgressMaterial;
    [SerializeField] private Material completeMaterial;

    [SerializeField] private bool isResetButton;

    private bool _activeButton;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _renderer = this.gameObject.GetComponent<Renderer>();
        _renderer.material = deactiveMaterial;

        _activeButton = false;

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
        EventsManager.OnResetButtons += ResetButton;
    }

    private void OnDisable()
    {
        EventsManager.OnResetButtons -= ResetButton;
    }

    [ClientRpc]
    public void SetMoveUpFalseClientRpc()
    {
        _moveUp = false;
    }

    [ClientRpc]
    public void SetMoveUpTrueClientRpc()
    {
        _activeButton = false;
        targetPosition = pushedPosition; 
        _moveUp = true;
    }

    [ClientRpc]
    public void SetMoveDownFalseClientRpc()
    {
        _moveDown = false;
    }

    [ClientRpc]
    public void SetMoveDownTrueClientRpc()
    {
        _activeButton = true;

        EventsManager.TriggerButton(shape, button);
        _isFullyPressed = false;

        targetPosition = originalPosition;

        _moveDown = true;

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = buttonAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 
        }
    }

    public void TriggerInteraction()
    {
        if (_isInteractable && !_activeButton)
        {
            TriggerInteractionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerInteractionServerRpc()
    {
        SetMoveDownTrueClientRpc();
    }

    private void ResetButton()
    {
        if(!_activeButton) return;
        ResetButtonServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetButtonServerRpc()
    {
        SetMoveUpTrueClientRpc();
    }

    public void EnableInteraction()
    {
        _isInteractable = true;
    }

    public void DisableInteraction()
    {
        _isInteractable = false;
    }

    //When a button is pressed, or is reset, this smoothly changes the buttons position and colour to be pushed in and blue if it is pressed.
    // This also change the button back to its original position and colour if it is reset
    private void FixedUpdate()
    {
        //Does not run if the button does not need to move
        if(!_moveDown && !_moveUp) return;

        //Slowly moves the button down, and changes the colour to blue
        if(_moveDown)
        {
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                targetPosition,
                Time.deltaTime * _buttonSpeed
            );

            _renderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    _renderer.material.GetColor("_BaseColor"),
                    inProgressMaterial.GetColor("_BaseColor"),
                    Time.deltaTime * _buttonSpeed
                )
            );

            if (Vector3.Distance(this.transform.position, targetPosition) <= _minDistance)
            {
                this.transform.position = targetPosition;
                _renderer.material = inProgressMaterial;

                _isFullyPressed = true;
                _moveDown = false;

                if(isResetButton)
                {
                    EventsManager.ActivateReset();
                }
            }

        //Slowly resets the button's position and colour
        }else if(_moveUp)
        {
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                targetPosition,
                Time.deltaTime * _buttonSpeed
            );

            _renderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    _renderer.material.GetColor("_BaseColor"),
                    deactiveMaterial.GetColor("_BaseColor"),
                    Time.deltaTime * _buttonSpeed
                )
            );

            if (Mathf.Abs(this.transform.position.y - targetPosition.y) <= _minDistance)
            {
                this.transform.position = targetPosition;
                _renderer.material = deactiveMaterial;

                _isFullyPressed = false;
                _moveDown = false;
                _moveUp = false;
            }
        }
    }
}
