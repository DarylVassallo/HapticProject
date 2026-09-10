using UnityEngine;

using Unity.Netcode;
using System;

public class ControllerManager : NetworkBehaviour
{
    //0 LeftHand
    //1 LeftController
    //2 RightHand
    //3 RightController

    [SerializeField] private GameObject[] controllers;
    [SerializeField] private SkinnedMeshRenderer[] controllerVisuals;
    private bool[] activeControllers = new bool[4];

    private NetworkVariable<int> activateControllerNum = new(-1);
    // private NetworkVariable<int> deactivateControllerNum = new(-1);

    private NetworkVariable<bool> isLeftHandActive = new(false);
    private NetworkVariable<bool> isLeftControllerActive = new(false);
    private NetworkVariable<bool> isRightHandActive = new(false);
    private NetworkVariable<bool> isRightControllerActive = new(false);

    private bool isInNetwork = false;

    public override void OnNetworkSpawn()
    {
        isInNetwork = true;
        EventsManager.CheckControllers();

        if(isLeftHandActive.Value) ActivateLeftHandRpc();
        if(isLeftControllerActive.Value) ActivateLeftControllerRpc();
        if(isRightHandActive.Value) ActivateRightHandRpc();
        if(isRightControllerActive.Value) ActivateRightControllerRpc();
    }

    private void OnEnable()
    {
        EventsManager.OnActivateController += ActivateController;
        // EventsManager.OnDeactivateController += DeactivateController;
        // EventsManager.OnToggleAll += ToggleHands;

        activateControllerNum.OnValueChanged += ActivateControllerChanged;
        // deactivateControllerNum.OnValueChanged += DeactivateControllerChanged;
    }

    private void OnDisable()
    {
        EventsManager.OnActivateController -= ActivateController;
        // EventsManager.OnDeactivateController -= DeactivateController;
        // EventsManager.OnToggleAll -= ToggleHands;

        activateControllerNum.OnValueChanged -= ActivateControllerChanged;
        // deactivateControllerNum.OnValueChanged -= DeactivateControllerChanged;
    }

    private void ActivateController(int _newActivateControllerNum)
    {
        if(isInNetwork) SetActivateControllerNumServerRpc(_newActivateControllerNum);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetActivateControllerNumServerRpc(int _newController)
    {
        activateControllerNum.Value = _newController;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsLeftHandActiveServerRpc(bool _isActive)
    {
        isLeftHandActive.Value = _isActive;
        if(_isActive) SetIsLeftControllerActiveServerRpc(false);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsLeftControllerActiveServerRpc(bool _isActive)
    {
        isLeftControllerActive.Value = _isActive;
        if(_isActive) SetIsLeftHandActiveServerRpc(false);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsRightHandActiveServerRpc(bool _isActive)
    {
        isRightHandActive.Value = _isActive;
        if(_isActive) SetIsRightControllerActiveServerRpc(false);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsRightControllerActiveServerRpc(bool _isActive)
    {
        isRightControllerActive.Value = _isActive;
        if(_isActive) SetIsRightHandActiveServerRpc(false);
    }

    //Activates the specified controller (based on the number used)
    private void ActivateControllerChanged(int previous, int current)
    {
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

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateLeftHandRpc()
    {
        SetIsLeftHandActiveServerRpc(true);

        ToggleControllers(0, true, true);
        ToggleControllers(1, false, false);
        ToggleControllers(2, true, false);
        ToggleControllers(3, false, false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateLeftControllerRpc()
    {
        SetIsLeftControllerActiveServerRpc(true);

        ToggleControllers(0, false, false);
        ToggleControllers(1, true, true);
        ToggleControllers(2, false, false);
        ToggleControllers(3, true, false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateRightHandRpc()
    {
        SetIsRightHandActiveServerRpc(true);
        
        ToggleControllers(0, true, true);
        ToggleControllers(1, false, false);
        ToggleControllers(2, true, false);
        ToggleControllers(3, false, false);
        
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateRightControllerRpc()
    {
        SetIsRightControllerActiveServerRpc(true);

        ToggleControllers(0, false, false);
        ToggleControllers(1, true, false);
        ToggleControllers(2, false, false);
        ToggleControllers(3, true, false);
    }



    // private void DeactivateController(int _newDeactivateControllerNum)
    // {
    //     if(isInNetwork) SetDeactivateControllerNumServerRpc(_newDeactivateControllerNum);
    // }

    // [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    // private void SetDeactivateControllerNumServerRpc(int _newController)
    // {
    //     deactivateControllerNum.Value = _newController;
    // }

    // //Deactivates the specified controller (based on the number used)
    // private void DeactivateControllerChanged(int previous, int current)
    // {
    //     switch(current)
    //     {
    //         case 0:
    //             DeactivateLeftHandRpc();
    //             break;
    //         case 1:
    //             DeactivateLeftControllerRpc();
    //             break;
    //         case 2:
    //             DeactivateRightHandRpc();
    //             break;
    //         case 3:
    //             DeactivateRightControllerRpc();
    //             break;
    //     }
    // }

    // [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    // public void DeactivateLeftHandRpc()
    // {
    //     ToggleControllers(0, false, false);
    //     ToggleControllers(1, true, true);
    //     ToggleControllers(2, false, false);
    //     ToggleControllers(3, true, false);
    // }

    // [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    // public void DeactivateLeftControllerRpc()
    // {
    //     ToggleControllers(0, true, true);
    //     ToggleControllers(1, false, false);
    //     ToggleControllers(2, true, false);
    //     ToggleControllers(3, false, false);
    // }

    // [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    // public void DeactivateRightHandRpc()
    // {
    //     ToggleControllers(0, false, false);
    //     ToggleControllers(1, true, true);
    //     ToggleControllers(2, false, false);
    //     ToggleControllers(3, true, false);
    // }

    // [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    // public void DeactivateRightControllerRpc()
    // {
    //     ToggleControllers(0, true, true);
    //     ToggleControllers(1, false, false);
    //     ToggleControllers(2, true, false);
    //     ToggleControllers(3, false, false);
    // }

    //Deactivates controllers if alternate controllers are used (for example, the Left Controller will be deactivated, if the Left Hand is activated)
    private void ToggleControllers(int _controllerNum, bool _activate, bool _changeVRFlashlight)
    {
        controllers[_controllerNum].SetActive(_activate);
        activeControllers[_controllerNum] = _activate;
        
        if((_controllerNum == 0 || _controllerNum == 2) && _activate)
        {
            controllers[_controllerNum + 1].SetActive(false);
            activeControllers[_controllerNum + 1] = false;
        }else if((_controllerNum == 1 || _controllerNum == 3) && _activate)
        {
            controllers[_controllerNum - 1].SetActive(false);
            activeControllers[_controllerNum - 1] = false;
        }

        EventsManager.ChangedControllers();

        if(_changeVRFlashlight) EventsManager.UpdateVRFlashlight();
    }

    //Deactivates / Activates all controllers
    private void ToggleHands(bool toggle)
    {
        if(toggle)
        {
            for(int i = 0; i < controllers.Length; i++)
            {
                controllers[i].SetActive(activeControllers[i]);
                if(controllerVisuals[i] != null) controllerVisuals[i].enabled = true;
            }
        }else{
            for(int i = 0; i < controllers.Length; i++)
            {
                controllers[i].SetActive(false);
                if(controllerVisuals[i] != null) controllerVisuals[i].enabled = false;
            }
        }
    }
}
