using UnityEngine;
using System;

public class SwitchPhysicsControllers : MonoBehaviour
{
    public GameObject rightHandPhysics;
    public GameObject leftHandPhysics;

    public GameObject rightControllerPhysics;
    public GameObject leftControllerPhysics;

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
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;
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
    
    public void TogglePhysicsControllers(bool isUsingVirtualHands)
    {
        if(_canPCFunction && _canVRFunction)
        {
            rightHandPhysics.SetActive(isUsingVirtualHands);
            leftHandPhysics.SetActive(isUsingVirtualHands);

            rightControllerPhysics.SetActive(!isUsingVirtualHands);
            leftControllerPhysics.SetActive(!isUsingVirtualHands);

            OnChangedControllers?.Invoke();
        }
    }
}
