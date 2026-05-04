using UnityEngine;

public class Platform : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(this.transform);
    }
    
    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
    }
}
