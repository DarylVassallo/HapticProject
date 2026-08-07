using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance;
    private Camera _pcPlayerCamera;
    private Ray _ray;
    private RaycastHit _hit;

    private void Awake()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MainCamera"))
        {
            if (obj.layer == LayerMask.NameToLayer("PCCamera"))
            {
                _pcPlayerCamera = obj.GetComponent<Camera>();
                break;
            }
        }        
    }

    private void OnEnable()
    {
        EventsManager.OnInteract += Interact;
    }

    private void OnDisable()
    {
        EventsManager.OnInteract -= Interact;
    }

    //Upon interacting, this checks if there is an object in front of the PC Player, and triggers the object's interaction function if there is
    private void Interact()
    {
        _ray = _pcPlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(_ray, out _hit, interactionDistance))
        {
            if(_hit.collider.GetComponent<IInteractable>() != null)
            {
                _hit.collider.GetComponent<IInteractable>().TriggerInteraction();
            }
        }
    }
}
