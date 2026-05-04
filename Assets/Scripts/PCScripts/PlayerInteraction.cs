using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance;
    private Camera _pcPlayerCamera;
    private Ray _ray;
    private RaycastHit _hit;

    private void Awake()
    {
        _pcPlayerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void OnEnable()
    {
        PCPlayerInputManager.OnInteract += Interact;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnInteract -= Interact;
    }

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
