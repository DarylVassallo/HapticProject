using UnityEngine;
using System;
public class ChargeStation : MonoBehaviour
{
    public static event Action OnCharge;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    void Start()
    {
        socket = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    void FixedUpdate()
    {
        if (!socket.hasSelection) return;
        
        OnCharge?.Invoke();
    }
}
