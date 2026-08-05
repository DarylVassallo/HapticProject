using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System;
public class ChargeStation : MonoBehaviour
{
    public static event Action<bool> OnCharge;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    void Awake()
    {
        socket = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        socket.selectEntered.AddListener(OnInserted);
        socket.selectExited.AddListener(OnRemoved);
    }

    private void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnInserted);
        socket.selectExited.RemoveListener(OnRemoved);
    }
    private void OnInserted(SelectEnterEventArgs args)
    {
        Debug.Log("ONINSERTED");
        OnCharge?.Invoke(true);
    }

    private void OnRemoved(SelectExitEventArgs args)
    {
        Debug.Log("ONREMOVED");
        OnCharge?.Invoke(false);
    }

    // void FixedUpdate()
    // {
    //     Debug.Log("socket.hasSelection: " + socket.hasSelection);
    //     if (!socket.hasSelection) return;
    //     Debug.Log("Charge");
    //     OnCharge?.Invoke();
    // }
}
