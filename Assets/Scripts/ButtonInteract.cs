using UnityEngine;
using System;
using System.Collections.Generic;
//This script controls is the button can be interacted with, and for what switch it is for
public class ButtonInteract : MonoBehaviour, IInteractable
{    
    public static event Action<MazeManager.ShapeType> OnTriggerButton;
    private bool _isInteractable = true;    

    [SerializeField] private MazeManager.ShapeType shape;

    [SerializeField] private float pressedDistance = 0.1f;
    private bool _isFullyPressed = false;
    private Vector3 pushedPosition;

    [SerializeField] private float _buttonSpeed = 20f;

    private bool _moveDown = false;
    private bool _moveUp = false;

    [SerializeField] private float _minDistance = 0.005f;

    public void TriggerInteraction()
    {
        Debug.Log("TriggerInteraction");
        Debug.Log("_isInteractable: " + _isInteractable);
        if (_isInteractable)
        {
            OnTriggerButton?.Invoke(shape);
            _isFullyPressed = false;
            pushedPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y - pressedDistance, this.transform.localPosition.z); 
            _moveDown = true;
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
            this.transform.localPosition = Vector3.Lerp(
                this.transform.localPosition,
                pushedPosition,
                Time.deltaTime * _buttonSpeed
            );

            Debug.Log("Going Down: " + this.transform.localPosition);

            if (Mathf.Abs(this.transform.localPosition.y - pushedPosition.y) <= _minDistance)
            {
                this.transform.localPosition = pushedPosition;
                pushedPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + pressedDistance, this.transform.localPosition.z);
                _isFullyPressed = true;
                _moveDown = false;
                _moveUp = true;
            }
        }
        
        if(_moveUp)
        {
            this.transform.localPosition = Vector3.Lerp(
                this.transform.localPosition,
                pushedPosition,
                Time.deltaTime * _buttonSpeed
            );

            Debug.Log("Going Up: " + this.transform.localPosition);

            if (Mathf.Abs(this.transform.localPosition.y - pushedPosition.y) <= _minDistance)
            {
                this.transform.localPosition = pushedPosition;
                _isFullyPressed = false;
                _moveDown = false;
                _moveUp = false;
            }
        }
    }
}
