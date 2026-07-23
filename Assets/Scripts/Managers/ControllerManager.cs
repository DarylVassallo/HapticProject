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
    [SerializeField] private GameObject[] controllers;

    private NetworkVariable<int> activateControllerNum = new(-1);
    private NetworkVariable<int> deactivateControllerNum = new(-1);

    private void OnEnable()
    {
        TogglePhysicsControllers.OnActivateController += ActivateController;
        TogglePhysicsControllers.OnDeactivateController += DeactivateController;
        MenuManager.OnToggleAll += ToggleHands;

        activateControllerNum.OnValueChanged += ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged += DeactivateControllerChanged;
    }

    private void OnDisable()
    {
        TogglePhysicsControllers.OnActivateController -= ActivateController;
        TogglePhysicsControllers.OnDeactivateController -= DeactivateController;
        MenuManager.OnToggleAll -= ToggleHands;

        activateControllerNum.OnValueChanged -= ActivateControllerChanged;
        deactivateControllerNum.OnValueChanged -= DeactivateControllerChanged;
    }

    private void ActivateControllerChanged(int previous, int current)
    {
        ActivateControllerRpc();
    }

    private void DeactivateControllerChanged(int previous, int current)
    {
        DeactivateControllerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivateControllerNumServerRpc(int _newController)
    {
        activateControllerNum.Value = _newController;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDeactivateControllerNumServerRpc(int _newController)
    {
        deactivateControllerNum.Value = _newController;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateControllerRpc()
    {
        controllers[activateControllerNum.Value].SetActive(true);
        OnChangedControllers?.Invoke();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateControllerRpc()
    {
        controllers[deactivateControllerNum.Value].SetActive(false);
        OnChangedControllers?.Invoke();
    }

    private void ActivateController(int _newActivateControllerNum)
    {
        Debug.Log("ControllerManager ActivateController: " + activateControllerNum);
        SetActivateControllerNumServerRpc(_newActivateControllerNum);
    }

    private void DeactivateController(int _newDeactivateControllerNum)
    {
        Debug.Log("ControllerManager DeactivateController: " + deactivateControllerNum);
        SetDeactivateControllerNumServerRpc(_newDeactivateControllerNum);
    }

    private void ToggleHands(bool toggle)
    {
        for(int i = 0; i < controllers.Length; i++)
        {
            controllers[i].SetActive(false);
        }
    }
}
