using UnityEngine;
using System;
using System.Collections.Generic;
//This script controls is the button can be interacted with, and for what switch it is for
public class ButtonInteract : MonoBehaviour, IInteractable
{    
    public static event Action<MazeManager.ShapeType> OnTriggerButton;
    private bool _isInteractable = true;    

    [SerializeField] private MazeManager.ShapeType shape;

    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            OnTriggerButton?.Invoke(shape);
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
}
