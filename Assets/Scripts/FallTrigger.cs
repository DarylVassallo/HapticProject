using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    private Health _entityHealth;
    private Respawn _entityRespawn;

    [SerializeField] private bool isVRTrigger;

    private void OnTriggerEnter(Collider other)
    {        
        Debug.Log(this.gameObject + " : " + other + " : OnTriggerEnter");
        if (isVRTrigger)
        {
            _entityRespawn = other.GetComponent<Respawn>();

            if(_entityRespawn != null)
            {
                _entityRespawn.ActivateRespawn();
            }
        }
        else
        {
            _entityHealth = other.GetComponent<Health>();
            Debug.Log("_entityHealth : " + _entityHealth);

            if (_entityHealth != null)
            {
                _entityHealth.ChangeHealth(-999, -1);
            }
            else
            {
                _entityRespawn = other.GetComponent<Respawn>();
                Debug.Log("_entityRespawn : " + _entityRespawn);

                if(_entityRespawn != null)
                {
                    _entityRespawn.ActivateRespawn();
                }
                else
                {
                    Destroy(other);
                }
            }
        }
        
    }
}
