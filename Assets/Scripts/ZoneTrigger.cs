using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private bool chargeTutorial;
    [SerializeField] private bool interactTutorial;

    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("PCPlayer"))
        { 
            if(chargeTutorial) EventsManager.TriggerPCChargeTutorial();
            if(interactTutorial) EventsManager.TriggerPCInteractTutorial();
        }
    }
}
