using UnityEngine;

using System;

public class FallTrigger : MonoBehaviour
{
    private Respawn _entityRespawn;

    [SerializeField] private bool isVRTrigger;

    public static event Action<GameObject, float> OnChangeHealth;

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
            OnChangeHealth?.Invoke(other.gameObject, -999);

            _entityRespawn = other.GetComponent<Respawn>();

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
