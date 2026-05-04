using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    private Health _entityHealth;
    private void OnTriggerEnter(Collider other)
    {
        _entityHealth = other.GetComponent<Health>();
        if (_entityHealth != null)
        {
            _entityHealth.ChangeHealth(-999, -1);
        }
        else
        {
            Destroy(other);
        }
    }
}
