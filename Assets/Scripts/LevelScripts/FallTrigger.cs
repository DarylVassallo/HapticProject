using UnityEngine;

using System;

public class FallTrigger : MonoBehaviour
{
    private Respawn _entityRespawn;

    [SerializeField] private bool isVRTrigger;
    
    private bool _canPCTrigger;

    private void Awake()
    {
        TogglePCTrigger(true);
    }

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
        Debug.Log("TogglePCTrigger: " + _toggle);
        _canPCTrigger = _toggle;
    }

    private void OnTriggerEnter(Collider other)
    {        
        Debug.Log("OnTriggerEnter: other: " + other);
        Debug.Log("other.CompareTag(PCPlayer): " + other.CompareTag("PCPlayer"));
        Debug.Log("_canPCTrigger: " + _canPCTrigger);
        Debug.Log("isVRTrigger: " + isVRTrigger);
        if(other.CompareTag("PCPlayer") && !_canPCTrigger) return;

        Debug.Log("2 OnTriggerEnter");

        //This kills the entity (if it is the PC Player or enemy)
        if(!isVRTrigger) EventsManager.ChangeHealthForEntity(other.gameObject, -999);
       
    }
}
