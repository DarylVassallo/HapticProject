using UnityEngine;

public class ButtonInteract : MonoBehaviour, IInteractable
{
    private Transform _pcPlayerTransform;
    private float _distance;

    [SerializeField] private GameObject symbol;

    // void Awake()
    // {        
    //     _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
    // }

    // private void OnEnable()
    // {
    //     PCPlayerInputManager.OnInteract += Interact;
    // }

    // private void OnDisable()
    // {
    //     PCPlayerInputManager.OnInteract -= Interact;
    // }

    // private void Interact()
    // {
    //     _distance = (transform.position - _pcPlayerTransform.position).magnitude;
    //     Debug.Log("distance: " + _distance);
    //     if (_distance <= 2)
    //     {
    //         symbol.SetActive(!symbol.active);
    //     }
    // }

    public void TriggerInteraction()
    {
        symbol.SetActive(!symbol.active);
    }
}
