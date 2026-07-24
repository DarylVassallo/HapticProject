using UnityEngine;

using Unity.Netcode;
using System;

public class ControllerManager : NetworkBehaviour
{
    //0 LeftHand
    //1 LeftController
    //2 RightHand
    //3 RightController

    public static event Action OnChangedControllers;
    public static event Action OnCheckControllers;
    [SerializeField] private GameObject[] controllers;

    private NetworkVariable<int> activateControllerNum = new(-1);
    private NetworkVariable<int> deactivateControllerNum = new(-1);

    private bool isInNetwork = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log("ControllerManager OnNetworkSpawn");

        isInNetwork = true;
        OnCheckControllers?.Invoke();
    }

    private void OnEnable()
    {
        Debug.Log("ControllerManager OnEnable");

        TogglePhysicsControllers.OnActivateController += ActivateController;
        TogglePhysicsControllers.OnDeactivateController += DeactivateController;
        MenuManager.OnToggleAll += ToggleHands;

        activateControllerNum.OnValueChanged += ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged += DeactivateControllerChanged;
    }

    private void OnDisable()
    {
        Debug.Log("ControllerManager OnDisable");

        TogglePhysicsControllers.OnActivateController -= ActivateController;
        TogglePhysicsControllers.OnDeactivateController -= DeactivateController;
        MenuManager.OnToggleAll -= ToggleHands;

        activateControllerNum.OnValueChanged -= ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged -= DeactivateControllerChanged;
    }

    private void ActivateController(int _newActivateControllerNum)
    {
        Debug.Log("ControllerManager ActivateController: " + _newActivateControllerNum);

        Debug.Log("ControllerManager ActivateController: " + activateControllerNum);
        if(isInNetwork) SetActivateControllerNumServerRpc(_newActivateControllerNum);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivateControllerNumServerRpc(int _newController)
    {
        Debug.Log("ControllerManager SetActivateControllerNumServerRpc: " + _newController);

        activateControllerNum.Value = _newController;
    }

    private void ActivateControllerChanged(int previous, int current)
    {
        Debug.Log("ControllerManager ActivateControllerChanged: " + current);

        ActivateControllerRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateControllerRpc()
    {
        Debug.Log("ControllerManager ActivateControllerRpc");

        controllers[activateControllerNum.Value].SetActive(true);
        
        if(activateControllerNum.Value == 0 || activateControllerNum.Value == 2)
        {
            controllers[activateControllerNum.Value + 1].SetActive(false);
        }else if(activateControllerNum.Value == 1 || activateControllerNum.Value == 3)
        {
            controllers[activateControllerNum.Value - 1].SetActive(false);
        }

        OnChangedControllers?.Invoke();
    }




    [ServerRpc(RequireOwnership = false)]
    private void SetDeactivateControllerNumServerRpc(int _newController)
    {
        Debug.Log("ControllerManager SetDeactivateControllerNumServerRpc: " + _newController);

        deactivateControllerNum.Value = _newController;
    }

    private void DeactivateController(int _newDeactivateControllerNum)
    {
        Debug.Log("ControllerManager DeactivateController: " + deactivateControllerNum);

        if(isInNetwork) SetDeactivateControllerNumServerRpc(_newDeactivateControllerNum);
    }

    private void DeactivateControllerChanged(int previous, int current)
    {
        Debug.Log("ControllerManager DeactivateControllerChanged: " + current);

        DeactivateControllerRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateControllerRpc()
    {
        Debug.Log("ControllerManager DeactivateControllerRpc");

        controllers[deactivateControllerNum.Value].SetActive(false);
        OnChangedControllers?.Invoke();
    }

    private void ToggleHands(bool toggle)
    {
        Debug.Log("ControllerManager ToggleHands: " + toggle);

        for(int i = 0; i < controllers.Length; i++)
        {
            controllers[i].SetActive(false);
        }
    }
}
