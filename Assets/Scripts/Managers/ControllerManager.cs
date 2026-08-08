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

    private NetworkVariable<int> activateControllerNum = new(-1);
    private NetworkVariable<int> deactivateControllerNum = new(-1);

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
        EventsManager.OnDeactivateController += DeactivateController;
        EventsManager.OnToggleAll += ToggleHands;

        activateControllerNum.OnValueChanged += ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged += DeactivateControllerChanged;
    }

    private void OnDisable()
    {
        EventsManager.OnActivateController -= ActivateController;
        EventsManager.OnDeactivateController -= DeactivateController;
        EventsManager.OnToggleAll -= ToggleHands;

        activateControllerNum.OnValueChanged -= ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged -= DeactivateControllerChanged;
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
        ToggleControllers(0, true);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateLeftControllerRpc()
    {
        SetIsLeftControllerActiveServerRpc(true);
        ToggleControllers(1, true);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateRightHandRpc()
    {
        SetIsRightHandActiveServerRpc(true);
        ToggleControllers(2, true);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateRightControllerRpc()
    {
        SetIsRightControllerActiveServerRpc(true);
        ToggleControllers(3, true);
    }



    private void DeactivateController(int _newDeactivateControllerNum)
    {
        if(isInNetwork) SetDeactivateControllerNumServerRpc(_newDeactivateControllerNum);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetDeactivateControllerNumServerRpc(int _newController)
    {
        deactivateControllerNum.Value = _newController;
    }

    //Deactivates the specified controller (based on the number used)
    private void DeactivateControllerChanged(int previous, int current)
    {
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

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeactivateLeftHandRpc()
    {
        ToggleControllers(0, false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeactivateLeftControllerRpc()
    {
        ToggleControllers(1, false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeactivateRightHandRpc()
    {
        ToggleControllers(2, false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeactivateRightControllerRpc()
    {
        ToggleControllers(3, false);
    }

    //Deactivates controllers if alternate controllers are used (for example, the Left Controller will be deactivated, if the Left Hand is activated)
    private void ToggleControllers(int _controllerNum, bool _activate)
    {
        controllers[_controllerNum].SetActive(_activate);
        
        if((_controllerNum == 0 || _controllerNum == 2) && _activate)
        {
            controllers[_controllerNum + 1].SetActive(false);
        }else if((_controllerNum == 1 || _controllerNum == 3) && _activate)
        {
            controllers[_controllerNum - 1].SetActive(false);
        }

        EventsManager.ChangedControllers();
    }

    //Deactivates all controllers
    private void ToggleHands(bool toggle)
    {
        for(int i = 0; i < controllers.Length; i++)
        {
            controllers[i].SetActive(false);
        }
    }
}
