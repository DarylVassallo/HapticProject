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
        if(hasBeenEnabled) ActivateControllerRpc();
    }

    private void OnNetworkDespawn()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnNetworkDespawn");

        hasNetworkSpawned = false;
        if(hasBeenEnabled) DeactivateControllerRpc();
    }

    private void OnEnable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnEnable");

        hasBeenEnabled = true;

        if(hasNetworkSpawned) ActivateControllerRpc();
    }

    private void OnDisable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnDisable");

        hasBeenEnabled = false;

        if(hasNetworkSpawned) DeactivateControllerRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateControllerRpc()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateControllerRpc");
        
        MenuManager.OnToggleAll += ToggleHands;

        isActive = true;
        controllerPhysics.SetActive(true);
        controllerRenderer.SetActive(true);

        OnChangedControllers?.Invoke();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateControllerRpc()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers DeactivateControllerRpc");

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
