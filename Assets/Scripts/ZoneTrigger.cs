using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private bool chargeTutorial;
    [SerializeField] private bool interactTutorial;
    [SerializeField] private bool enteredTemple;

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
        if (other.CompareTag("PCPlayer"))
        { 
            if(chargeTutorial) EventsManager.TriggerPCChargeTutorial();
            if(interactTutorial) EventsManager.TriggerPCInteractTutorial();
            if(enteredTemple && _canPCTrigger) EventsManager.EnteredTemple(true);
        }
    }
}
