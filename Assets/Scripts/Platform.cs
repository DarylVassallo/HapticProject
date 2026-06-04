using UnityEngine;

public class Platform : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            other.transform.SetParent(this.transform);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PCPlayer") || other.CompareTag("Enemy"))
        {
            other.transform.SetParent(null);
        }
    }
}
