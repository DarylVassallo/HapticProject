using UnityEngine;

using System;

public class FallTrigger : MonoBehaviour
{
    private Respawn _entityRespawn;

    [SerializeField] private bool isVRTrigger;
    
    private bool _canPCTrigger;

    private void OnEnable()
    {
        EventsManager.OnTogglePCTrigger += TogglePCTrigger;
    }

    private void OnDisable()
    {
        EventsManager.OnTogglePCTrigger -= TogglePCTrigger;
    }

    private void TogglePCTrigger(bool _toggle)
    {
        _canPCTrigger = _toggle;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.CompareTag("PCPlayer") && !_canPCTrigger) return;

        //This kills the entity (if it is the PC Player or enemy)
        if(!isVRTrigger) EventsManager.ChangeHealthForEntity(other.gameObject, -999);
       
    }
}
