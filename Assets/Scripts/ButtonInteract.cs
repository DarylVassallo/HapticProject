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

    private float pressedDistance = 0.2f;
    private bool _isFullyPressed = false;
    private Vector3 pushedPosition;

    private float _buttonSpeed = 20f;

    private bool _moveDown = false;
    private bool _moveUp = false;

    private float _minDistance = 0.005f;

    public void TriggerInteraction()
    {
        Debug.Log("TriggerInteraction");
        Debug.Log("_isInteractable: " + _isInteractable);
        if (_isInteractable)
        {
            OnTriggerButton?.Invoke(shape, button);
            _isFullyPressed = false;
            //This line formed with ChatGPT
            pushedPosition =    this.transform.position + 
                                (   -transform.up * 
                                    pressedDistance * 
                                    this.transform.localScale.x
                                );            
            _moveDown = true;
        }
    }

    public void EnableInteraction()
    {
        Debug.Log("EnableInteraction");
        _isInteractable = true;
    }

    public void DisableInteraction()
    {
        Debug.Log("DisableInteraction");
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

            if (Vector3.Distance(this.transform.position, pushedPosition) <= _minDistance)
            {
                this.transform.position = pushedPosition;
                pushedPosition =    this.transform.position + 
                                    (   transform.up * 
                                        pressedDistance * 
                                        this.transform.localScale.x
                                    );
                _isFullyPressed = true;
                _moveDown = false;
                _moveUp = true;
            }
        }
        
        if(_moveUp)
        {
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                pushedPosition,
                Time.deltaTime * _buttonSpeed
            );

            if (Mathf.Abs(this.transform.position.y - pushedPosition.y) <= _minDistance)
            {
                this.transform.position = pushedPosition;
                _isFullyPressed = false;
                _moveDown = false;
                _moveUp = false;
            }
        }
    }
}
