using UnityEngine;
using System;

public class SwitchPhysicsControllers : MonoBehaviour
{
    public GameObject rightHandPhysics;
    public GameObject leftHandPhysics;

    public GameObject rightControllerPhysics;
    public GameObject leftControllerPhysics;

    public static event Action OnChangedControllers;

    private void Start()
    {
        rightHandPhysics.SetActive(false);
        leftHandPhysics.SetActive(false);
        
        rightControllerPhysics.SetActive(false);
        leftControllerPhysics.SetActive(false);
    }
    
    public void TogglePhysicsControllers(bool isUsingVirtualHands)
    {
        rightHandPhysics.SetActive(isUsingVirtualHands);
        leftHandPhysics.SetActive(isUsingVirtualHands);

        rightControllerPhysics.SetActive(!isUsingVirtualHands);
        leftControllerPhysics.SetActive(!isUsingVirtualHands);

        OnChangedControllers?.Invoke();
    }
}
