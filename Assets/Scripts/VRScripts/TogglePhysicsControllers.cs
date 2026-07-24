using UnityEngine;
using System;

public class TogglePhysicsControllers : MonoBehaviour
{
    private bool isActive = false;
    // private bool isInNetwork = false;

    [SerializeField] private GameObject controllerPhysics;
    [SerializeField] private GameObject controllerRenderer;

    public static event Action<int> OnActivateController;
    public static event Action<int> OnDeactivateController;

    [SerializeField] private int controllerNum;
    //0 LeftHand
    //1 LeftController
    //2 RightHand
    //3 RightController

    private bool hasBeenEnabled = false;

    private void OnEnable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnEnable");

        ControllerManager.OnCheckControllers += CheckController;
        hasBeenEnabled = true;
        ActivateController();
    }

    private void OnDisable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnDisable");

        ControllerManager.OnCheckControllers -= CheckController;
        hasBeenEnabled = false;
        DeactivateController();
    }

    private void CheckController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers CheckController");
        
        if(hasBeenEnabled)
        {
            ActivateController();
        }
        else
        {
            DeactivateController();
        }
    }
    
    private void ActivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateController");

        OnActivateController?.Invoke(controllerNum);
    }

    private void DeactivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers DeactivateController");

        OnDeactivateController?.Invoke(controllerNum);
    }
}
