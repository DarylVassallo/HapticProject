using UnityEngine;
using System;

public class SwitchPhysicsControllers : MonoBehaviour
{
    public GameObject rightHandPhysics;
    public GameObject rightHandRenderer;
    public GameObject leftHandPhysics;
    public GameObject leftHandRenderer;

    public GameObject rightControllerPhysics;
    public GameObject rightControllerRenderer;
    public GameObject leftControllerPhysics;
    public GameObject leftControllerRenderer;

    public static event Action OnChangedControllers;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    // private void Start()
    // {
    //     rightHandPhysics.SetActive(false);
    //     leftHandPhysics.SetActive(false);
        
    //     rightControllerPhysics.SetActive(false);
    //     leftControllerPhysics.SetActive(false);
    // }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;

        MenuManager.OnToggleAll += ToggleHands;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;

        MenuManager.OnToggleAll -= ToggleHands;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
        }
    }

    private void GetVRPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("VRPlayer") != null)
        {
            _canVRFunction = true;
        }
    }

    private void ToggleHands(bool toggle)
    {
        rightHandRenderer.SetActive(toggle);
        leftHandRenderer.SetActive(toggle);

        rightControllerRenderer.SetActive(toggle);
        leftControllerRenderer.SetActive(toggle);
    }
    
    public void TogglePhysicsControllers(bool newIsUsingVirtualHands)
    {
        if(_canPCFunction && _canVRFunction)
        {
            rightHandPhysics.SetActive(newIsUsingVirtualHands);
            leftHandPhysics.SetActive(newIsUsingVirtualHands);

            rightControllerPhysics.SetActive(!newIsUsingVirtualHands);
            leftControllerPhysics.SetActive(!newIsUsingVirtualHands);

            OnChangedControllers?.Invoke();
        }
    }
}
