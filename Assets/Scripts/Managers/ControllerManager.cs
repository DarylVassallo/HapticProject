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
        // Debug.Log("ControllerManager ActivateController: " + _newActivateControllerNum);
        if(isInNetwork) SetActivateControllerNumServerRpc(_newActivateControllerNum);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivateControllerNumServerRpc(int _newController)
    {
        // Debug.Log("ControllerManager SetActivateControllerNumServerRpc: " + _newController);
        activateControllerNum.Value = _newController;
    }

    private void ActivateControllerChanged(int previous, int current)
    {
        Debug.Log("ControllerManager ActivateControllerChanged: " + current);

        switch(current)
        {
            case 0:
                ActivateLeftHandRpc();
                break;
            case 1:
                ActivateLeftControllerRpc();
                break;
            case 2:
                ActivateRightHandRpc();
                break;
            case 3:
                ActivateRightControllerRpc();
                break;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateLeftHandRpc()
    {
        Debug.Log("ControllerManager ActivateLeftHandRpc");
        ToggleControllers(0, true);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateLeftControllerRpc()
    {
        Debug.Log("ControllerManager ActivateLeftControllerRpc");
        ToggleControllers(1, true);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateRightHandRpc()
    {
        Debug.Log("ControllerManager ActivateRightHandRpc");
        ToggleControllers(2, true);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateRightControllerRpc()
    {
        Debug.Log("ControllerManager ActivateRightControllerRpc");
        ToggleControllers(3, true);
    }



    private void DeactivateController(int _newDeactivateControllerNum)
    {
        // Debug.Log("ControllerManager DeactivateController: " + deactivateControllerNum);
        if(isInNetwork) SetDeactivateControllerNumServerRpc(_newDeactivateControllerNum);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDeactivateControllerNumServerRpc(int _newController)
    {
        // Debug.Log("ControllerManager SetDeactivateControllerNumServerRpc: " + _newController);
        deactivateControllerNum.Value = _newController;
    }

    private void DeactivateControllerChanged(int previous, int current)
    {
        Debug.Log("ControllerManager DeactivateControllerChanged: " + current);

        switch(current)
        {
            case 0:
                DeactivateLeftHandRpc();
                break;
            case 1:
                DeactivateLeftControllerRpc();
                break;
            case 2:
                DeactivateRightHandRpc();
                break;
            case 3:
                DeactivateRightControllerRpc();
                break;
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateLeftHandRpc()
    {
        Debug.Log("ControllerManager DeactivateLeftHandRpc");
        ToggleControllers(0, false);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateLeftControllerRpc()
    {
        Debug.Log("ControllerManager DeactivateLeftControllerRpc");
        ToggleControllers(1, false);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateRightHandRpc()
    {
        Debug.Log("ControllerManager DeactivateRightHandRpc");
        ToggleControllers(2, false);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateRightControllerRpc()
    {
        Debug.Log("ControllerManager DeactivateRightControllerRpc");
        ToggleControllers(3, false);
    }

    private void ToggleControllers(int _controllerNum, bool _activate)
    {
        Debug.Log("ControllerManager ToggleControllers: " + _controllerNum + " : " + _activate);
        Debug.Log("ControllerManager ToggleControllers: controllers["  +_controllerNum +"]: " + controllers[_controllerNum]);

        Debug.Log("ControllerManager ToggleControllers: old controllers["  +_controllerNum +"].active: " + controllers[_controllerNum].active);
        controllers[_controllerNum].SetActive(_activate);
        Debug.Log("ControllerManager ToggleControllers: new controllers["  +_controllerNum +"].active: " + controllers[_controllerNum].active);
        
        if((_controllerNum == 0 || _controllerNum == 2) && _activate)
        {
            Debug.Log("ControllerManager ToggleControllers: other controllers["  + (_controllerNum + 1) +"]: " + controllers[(_controllerNum + 1)]);
            Debug.Log("ControllerManager ToggleControllers: old other controllers["  + (_controllerNum + 1) +"].active: " + controllers[(_controllerNum + 1)].active);
            controllers[_controllerNum + 1].SetActive(false);
            Debug.Log("ControllerManager ToggleControllers: new other controllers["  + (_controllerNum + 1) +"].active: " + controllers[(_controllerNum + 1)].active);
        }else if((_controllerNum == 1 || _controllerNum == 3) && _activate)
        {
            Debug.Log("ControllerManager ToggleControllers: other controllers["  + (_controllerNum - 1) +"]: " + controllers[(_controllerNum - 1)]);
            Debug.Log("ControllerManager ToggleControllers: old other controllers["  + (_controllerNum - 1) +"].active: " + controllers[(_controllerNum - 1)].active);
            controllers[_controllerNum - 1].SetActive(false);
            Debug.Log("ControllerManager ToggleControllers: new other controllers["  + (_controllerNum - 1) +"].active: " + controllers[(_controllerNum - 1)].active);
        }

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
