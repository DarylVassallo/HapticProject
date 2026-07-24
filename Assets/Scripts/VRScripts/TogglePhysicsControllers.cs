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
    
    // public override void OnNetworkSpawn()
    // {
    //     Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnNetworkSpawn");

    //     isInNetwork = true;
    //     if(isActive) ActivateControllerRpc();
    // }

    // private void OnNetworkDespawn()
    // {
    //     Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnNetworkDespawn");

    //     isInNetwork = false;
    //     if(!isActive) DeactivateControllerRpc();
    // }
    // private void GetPCPlayerData()
    // {
    //     Debug.Log(this.gameObject  + " : TogglePhysicsControllers GetPCPlayerData");
    //     isInNetwork = true;
    //     Debug.Log(this.gameObject  + " : TogglePhysicsControllers GetPCPlayerData isActive: " + isActive);
    //     if(isActive) ActivateControllerRpc();
    // }

    private void OnEnable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnEnable");
        // ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;

        hasBeenEnabled = true;

        ActivateController();
        // if(isInNetwork)
        // {
        //     ActivateControllerRpc();
        // }
        // else{
        //     ActivateController();
        // }
    }

    private void OnDisable()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers OnDisable");
        // ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;

        hasBeenEnabled = false;

        DeactivateController();
        // if(isInNetwork)
        // {
        //     DeactivateControllerRpc();
        // }
        // else{
        //     DeactivateController();
        // }
    }

    // [Rpc(SendTo.Everyone, RequireOwnership = false)]
    // public void ActivateControllerRpc()
    // {
    //     Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateControllerRpc");
    //     ActivateController();
    // }

    public void ActivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateController");

        OnActivateController?.Invoke(controllerNum);


        // Debug.Log(this.gameObject  + " : TogglePhysicsControllers ActivateController");
        
        // MenuManager.OnToggleAll += ToggleHands;

        // isActive = true;
        // controllerPhysics.SetActive(true);

        // OnChangedControllers?.Invoke();
    }

    // [Rpc(SendTo.Everyone, RequireOwnership = false)]
    // public void DeactivateControllerRpc()
    // {
    //     DeactivateController();
    // }

    public void DeactivateController()
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers DeactivateController");

        OnDeactivateController?.Invoke(controllerNum);

        // Debug.Log(this.gameObject  + " : TogglePhysicsControllers DeactivateController");

        // MenuManager.OnToggleAll -= ToggleHands;

        // isActive = false;
        // controllerPhysics.SetActive(false);
        
        // OnChangedControllers?.Invoke();
    }

     private void ToggleHands(bool toggle)
    {
        Debug.Log(this.gameObject  + " : TogglePhysicsControllers ToggleHands");
    }
}
