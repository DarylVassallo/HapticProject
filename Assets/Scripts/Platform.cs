using UnityEngine;
using Unity.Netcode;

public class Platform : NetworkBehaviour
{
    private Quaternion playerRotation;

    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;
        Debug.Log("On Platform");

        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            Debug.Log(this.gameObject + " : Enter Platform : " + other.gameObject);
            // playerRotation = other.transform.rotation;
            other.transform.SetParent(this.transform);
            // other.transform.rotation = playerRotation;
            // SetEntityParentServerRpc(other.transform, this.transform);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(!IsOwner) return;
        
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            Debug.Log(this.gameObject + " : Exit Platform : " + other.gameObject);
            // playerRotation = other.transform.rotation;
            other.transform.SetParent(null);
            // other.transform.rotation = playerRotation;
            // SetEntityParentServerRpc(other.transform, null);
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetEntityParentServerRpc(Transform entity, Transform _newParent)
    // {
    //     entity.SetParent(_newParent);
    // }
}
