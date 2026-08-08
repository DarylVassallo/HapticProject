using UnityEngine;
using Unity.Netcode;

public class Platform : NetworkBehaviour
{
    private Quaternion playerRotation;

    //If the PC Player / enemy moves onto the platform, they will be reparented to the platform, thus moving with it if it moves
    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy")) other.transform.SetParent(this.transform);
    }
    
    //If the PC Player / enemy moves off the platform, they will no longer parented to the platform
    private void OnTriggerExit(Collider other)
    {
        if(!IsOwner) return;
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy")) other.transform.SetParent(null);
    }
}
