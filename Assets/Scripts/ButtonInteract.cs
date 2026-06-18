using UnityEngine;
using System;
using System.Collections.Generic;

//This script controls is the button can be interacted with, and for what switch it is for
public class ButtonInteract : MonoBehaviour, IInteractable
{    
    public static event Action<MazeManager.ShapeType, MazeManager.ButtonType> OnTriggerButton;
    private bool _isInteractable = true;    

    [SerializeField] private MazeManager.ShapeType shape;
    [SerializeField] private MazeManager.ButtonType button;

    private float pressedDistance = 0.1f;
    private bool _isFullyPressed = false;
    private Vector3 pushedPosition;

    private float _buttonSpeed = 20f;

    private bool _moveDown = false;
    private bool _moveUp = false;

    private float _minDistance = 0.005f;

    [SerializeField] private AudioClip buttonAudio;
    private AudioSource _audioSource;
    private Renderer _renderer;

    [SerializeField] private Material deactiveMaterial;
    [SerializeField] private Material inProgressMaterial;
    [SerializeField] private Material completeMaterial;

    [SerializeField] private bool isResetButton;
    public static event Action OnActivateReset;

    private bool _activeButton;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _renderer = this.gameObject.GetComponent<Renderer>();
        _renderer.material = deactiveMaterial;

        _activeButton = false;
    }

    private void OnEnable()
    {
        MazeManager.OnResetHiddenButtons += ResetButton;
    }

    public void TriggerInteraction()
    {
        if (_isInteractable && !_activeButton)
        {
            _activeButton = true;

            OnTriggerButton?.Invoke(shape, button);
            _isFullyPressed = false;

            //This line formed with ChatGPT
            pushedPosition =    this.transform.position + 
                                (   -transform.forward * 
                                    pressedDistance * 
                                    this.transform.localScale.x
                                );            
            _moveDown = true;

            if(!_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = buttonAudio;
                _audioSource.Play();
                _audioSource.enabled = true; 
            }
        }
    }

    private void ResetButton()
    {
        if(!_isFullyPressed)
        {
            _activeButton = false;

            pushedPosition =    this.transform.position + 
                                (   transform.forward * 
                                    pressedDistance * 
                                    this.transform.localScale.x
                                );                  
            
            _moveUp = true;
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

    private void FixedUpdate()
    {
        if(!_moveDown && !_moveUp) return;

        if(_moveDown)
        {
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                pushedPosition,
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

            if (Vector3.Distance(this.transform.position, pushedPosition) <= _minDistance)
            {
                this.transform.position = pushedPosition;
                _renderer.material = inProgressMaterial;

                pushedPosition =    this.transform.position + 
                                    (   transform.forward * 
                                        pressedDistance * 
                                        this.transform.localScale.x
                                    );
                _isFullyPressed = true;
                _moveDown = false;
                // _moveUp = true;

                if(isResetButton)
                {
                    OnActivateReset?.Invoke();
                }
            }
        }else if(_moveUp)
        {
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                pushedPosition,
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

            if (Mathf.Abs(this.transform.position.y - pushedPosition.y) <= _minDistance)
            {
                this.transform.position = pushedPosition;
                _renderer.material = deactiveMaterial;

                _isFullyPressed = false;
                _moveDown = false;
                _moveUp = false;
            }
        }
    }
}
