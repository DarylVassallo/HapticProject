using UnityEngine;

using System;

public class FallTrigger : MonoBehaviour
{
    private Respawn _entityRespawn;

    [SerializeField] private bool isVRTrigger;


    private void OnTriggerEnter(Collider other)
    {        
        //This kills the entity (if it is the PC Player or enemy)
        if(!isVRTrigger) EventsManager.ChangeHealthForEntity(other.gameObject, -999);

        // //If this is a VR object, then the object is set back to it's initial position
        // _entityRespawn = other.GetComponent<Respawn>();

        // if(_entityRespawn != null)
        // {
        //     _entityRespawn.ActivateRespawn();
        // }
        // else
        // {
        //     Destroy(other);
        // }        
    }
}
