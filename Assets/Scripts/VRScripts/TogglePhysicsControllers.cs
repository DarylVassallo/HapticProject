using UnityEngine;
using System;

public class TogglePhysicsControllers : MonoBehaviour
{
    private bool isActive = false;
    [SerializeField] private GameObject controllerPhysics;
    [SerializeField] private GameObject controllerRenderer;

    public static event Action OnChangedControllers;

    private void OnEnable()
    {
        MenuManager.OnToggleAll += ToggleHands;

        isActive = true;
        controllerPhysics.SetActive(true);
        controllerRenderer.SetActive(true);

        OnChangedControllers?.Invoke();
    }

    private void OnDisable()
    {
        MenuManager.OnToggleAll -= ToggleHands;

        isActive = false;
        controllerPhysics.SetActive(false);
        controllerRenderer.SetActive(false);
        
        OnChangedControllers?.Invoke();
    }

     private void ToggleHands(bool toggle)
    {
        if (isActive) controllerRenderer.SetActive(toggle);
    }
}
