using UnityEngine;
using System;

public class TogglePhysicsControllers : MonoBehaviour
{
    private bool isActive = false;
    // private bool isInNetwork = false;

    [SerializeField] private GameObject controllerPhysics;
    [SerializeField] private GameObject controllerRenderer;

    [SerializeField] private int controllerNum;
    //0 LeftHand
    //1 LeftController
    //2 RightHand
    //3 RightController

    private bool hasBeenEnabled = false;

    private void OnEnable()
    {
        EventsManager.OnCheckControllers += CheckController;
        hasBeenEnabled = true;
        ActivateController();
    }

    private void OnDisable()
    {
        EventsManager.OnCheckControllers -= CheckController;
        hasBeenEnabled = false;
        DeactivateController();
    }

    //Checks if the current controller should be activated
    private void CheckController()
    {        
        if(hasBeenEnabled)
        {
            ActivateController();
        }else{
            DeactivateController();
        }
    }
    
    //Activates the specified controller
    private void ActivateController()
    {
        EventsManager.ActivateController(controllerNum);
    }

    //Deactivates the specified controller
    private void DeactivateController()
    {
        EventsManager.DeactivateController(controllerNum);
    }
}
