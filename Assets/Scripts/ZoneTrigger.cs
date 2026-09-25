using UnityEngine;
using System.Collections;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private bool chargeTutorial;

    [SerializeField] private bool interactTutorial;

    [SerializeField] private bool crossingCrookedBridge;
    private bool _crossedCrookedBridge;

    [SerializeField] private bool reachedEnd;

    [SerializeField] private bool enteredTemple;

    private bool _canPCTrigger;

    private void OnEnable()
    {
        EventsManager.OnTogglePCTrigger += TogglePCTrigger;
        EventsManager.OnUsedCrookedBridgeTeleporter += UsedFirstCrookedBridgeTeleporter;
    }

    private void OnDisable()
    {
        EventsManager.OnTogglePCTrigger -= TogglePCTrigger;
        EventsManager.OnUsedCrookedBridgeTeleporter -= UsedFirstCrookedBridgeTeleporter;
    }

    private void TogglePCTrigger(bool _toggle)
    {
        _canPCTrigger = _toggle;
    }

    private void UsedFirstCrookedBridgeTeleporter()
    {
        Debug.Log("UsedFirstCrookedBridgeTeleporter");
        StartCoroutine(CheckPCWaitingForCrookedBridge());
    }

    IEnumerator CheckPCWaitingForCrookedBridge()
    {
        Debug.Log("CheckPCWaitingForCrookedBridge");
        yield return new WaitForSeconds(5f);
        if(!_crossedCrookedBridge) EventsManager.CrookedBridgePCPlayerWaited();
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("PCPlayer"))
        { 
            if(chargeTutorial) EventsManager.TriggerPCChargeTutorial();
            if(interactTutorial) EventsManager.TriggerPCInteractTutorial();
            if(reachedEnd) EventsManager.PCReachedEnd();
            if(crossingCrookedBridge) _crossedCrookedBridge = true;
            if(enteredTemple && _canPCTrigger) EventsManager.EnteredTemple(true);
        }
    }
}
