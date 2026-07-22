using UnityEngine;
using System;

using Unity.Netcode;

public class TogglePhysicsControllers : NetworkBehaviour
{
    private bool isActive = false;
    [SerializeField] private GameObject controllerPhysics;
    [SerializeField] private GameObject controllerRenderer;

    public static event Action OnChangedControllers;

    private bool hasNetworkSpawned = false;
    private bool hasBeenEnabled = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnNetworkSpawn");

        hasNetworkSpawned = true;
        if(hasBeenEnabled) ActivateController();
    }

    private void OnNetworkDespawn()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnNetworkDespawn");

        hasNetworkSpawned = false;
        if(hasBeenEnabled) DeactivateController();
    }

    private void OnEnable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnEnable");

        hasBeenEnabled = true;

        if(hasNetworkSpawned) ActivateController();
    }

    private void OnDisable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnDisable");

        hasBeenEnabled = false;

        if(hasNetworkSpawned) DeactivateController();
    }

    private void ActivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateController");
        
        MenuManager.OnToggleAll += ToggleHands;

        isActive = true;
        controllerPhysics.SetActive(true);
        controllerRenderer.SetActive(true);

        OnChangedControllers?.Invoke();
    }

    private void DeactivateController()
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
