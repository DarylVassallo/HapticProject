using UnityEngine;
using System;

using Unity.Netcode;

public class TogglePhysicsControllers : NetworkBehaviour
{
    private bool isActive = false;
    [SerializeField] private GameObject controllerPhysics;
    [SerializeField] private GameObject controllerRenderer;

    public static event Action OnChangedControllers;

    private bool hasBeenEnabled = false;

    private void OnEnable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnEnable");

        hasBeenEnabled = true;

        ActivateController();
    }

    private void OnDisable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnDisable");

        hasBeenEnabled = false;

        DeactivateController();
    }

    public void ActivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateController");
        
        MenuManager.OnToggleAll += ToggleHands;

        isActive = true;
        controllerPhysics.SetActive(true);
        controllerRenderer.SetActive(true);

        OnChangedControllers?.Invoke();
    }

    public void DeactivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers DeactivateController");

        MenuManager.OnToggleAll -= ToggleHands;

        isActive = false;
        controllerPhysics.SetActive(false);
        controllerRenderer.SetActive(false);
        
        OnChangedControllers?.Invoke();
    }

     private void ToggleHands(bool toggle)
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ToggleHands");
        if (isActive) controllerRenderer.SetActive(toggle);
    }
}
