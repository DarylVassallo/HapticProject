using UnityEngine;
using Unity.Netcode;

public class Platform : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(!IsOwner) return;

        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            Debug.Log(this.gameObject + " : Enter Platform : " + other.gameObject);
            other.transform.SetParent(this.transform);
            // SetEntityParentServerRpc(other.transform, this.transform);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(!IsOwner) return;
        
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            Debug.Log(this.gameObject + " : Exit Platform : " + other.gameObject);
            other.transform.SetParent(null);
            // SetEntityParentServerRpc(other.transform, null);
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetEntityParentServerRpc(Transform entity, Transform _newParent)
    // {
    //     entity.SetParent(_newParent);
    // }
}
