using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableLayerControl : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _xrGrabInteractable;
    private int _standardLayer;
    private GameObject _interactor;
    private bool isRightHandInteractable = false;
    private bool isLeftHandInteractable = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor leftHandSocket;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor rightHandSocket;

    void Awake()
    {
        _xrGrabInteractable = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _xrGrabInteractable.selectEntered.AddListener(OnGrabbed);
        _xrGrabInteractable.selectExited.AddListener(OnReleased);

        _standardLayer = this.gameObject.layer;

        leftHandSocket = GameObject.FindWithTag("LeftHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        leftHandSocket.selectEntered.AddListener(OnLeftHandInserted);
        leftHandSocket.selectExited.AddListener(OnLeftHandRemoved);

        rightHandSocket = GameObject.FindWithTag("RightHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        rightHandSocket.selectEntered.AddListener(OnRightHandInserted);
        rightHandSocket.selectExited.AddListener(OnRightHandRemoved);
    }

    private void OnDestroy()
    {
        leftHandSocket.selectEntered.RemoveListener(OnLeftHandInserted);
        leftHandSocket.selectExited.RemoveListener(OnLeftHandRemoved);

        rightHandSocket.selectEntered.RemoveListener(OnRightHandInserted);
        rightHandSocket.selectExited.RemoveListener(OnRightHandRemoved);
    }

    private void OnLeftHandInserted(SelectEnterEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;

        Debug.Log("===================");
        Debug.Log(this.gameObject + " : OnLeftHandInserted");

        isLeftHandInteractable = true;
        ChangeLayer(this.gameObject, -1);

        ChangeInteractionLayer(1);
    }

    private void OnLeftHandRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;

        Debug.Log("===================");
        Debug.Log(this.gameObject + " : OnLeftHandRemoved");

        isLeftHandInteractable = false;
        ChangeLayer(this.gameObject, -1);

        ChangeInteractionLayer(0);
    }

    private void OnRightHandInserted(SelectEnterEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;

        Debug.Log("===================");
        Debug.Log(this.gameObject + " : OnRightHandInserted");

        isRightHandInteractable = true;
        ChangeLayer(this.gameObject, -1);
        
        ChangeInteractionLayer(-1);
    }

    private void OnRightHandRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;

        Debug.Log("===================");
        Debug.Log(this.gameObject + " : OnRightHandRemoved");

        isRightHandInteractable = false;
        ChangeLayer(this.gameObject, -1);

        ChangeInteractionLayer(0);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _interactor = args.interactorObject.transform.gameObject;

        // _standardLayer = this.gameObject.layer;

        if (_interactor.CompareTag("LeftHandInteractor"))
        {
            isLeftHandInteractable = true;

            if(isLeftHandInteractable && isRightHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }else if(isLeftHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }
        }else if (_interactor.CompareTag("RightHandInteractor"))
        {
            isRightHandInteractable = true;

            if(isLeftHandInteractable && isRightHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }else if(isRightHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _interactor = args.interactorObject.transform.gameObject;

        if (_interactor.CompareTag("LeftHandInteractor"))
        {
            isLeftHandInteractable = false;             
        }else if (_interactor.CompareTag("RightHandInteractor"))
        {
            isRightHandInteractable = false;
        }

        ChangeLayer(this.gameObject, -1);
    }

    public void ChangeLayer(GameObject currentObject, int layer)
    {
        Debug.Log("===================");
        if(layer == -1)
        {
            if(isLeftHandInteractable && isRightHandInteractable)
            {
                Debug.Log(this.gameObject + " : BothHandInteractable");
                ChangeInteractionLayer(0);
                layer = LayerMask.NameToLayer("BothHandInteractable");
            }else if(isLeftHandInteractable)
            {
                Debug.Log(this.gameObject + " : LeftHandInteractable");
                ChangeInteractionLayer(-1);
                layer = LayerMask.NameToLayer("LeftHandInteractable");
            }else if(isRightHandInteractable)
            {
                Debug.Log(this.gameObject + " : RightHandInteractable");
                ChangeInteractionLayer(1);
                layer = LayerMask.NameToLayer("RightHandInteractable");
            }
            else
            {
                Debug.Log(this.gameObject + " : None");
                // ChangeInteractionLayer(0);
                layer = _standardLayer;
            }
        }

        currentObject.layer = layer;

        foreach (Transform child in currentObject.transform)
        {
            ChangeLayer(child.gameObject, layer);
        }
    }

    private void ChangeInteractionLayer(int interactionLayer)
    {
        Debug.Log(this.gameObject + " : ChangeInteractionLayer");
        Debug.Log(this.gameObject + " : this.gameObject : " + this.gameObject);

        Debug.Log(this.gameObject + " : interactionLayer : " + interactionLayer);
        Debug.Log(this.gameObject + " : old _xrGrabInteractable.interactionLayers.value : " + _xrGrabInteractable.interactionLayers.value);

        if(interactionLayer == -1)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("LeftHandOnlyGrabbable");
        }else if(interactionLayer == 0)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default");
        }else if(interactionLayer == 1)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("RightHandOnlyGrabbable");
        }

        Debug.Log(this.gameObject + " : new interactionLayer : " + interactionLayer);
        Debug.Log(this.gameObject + " : new _xrGrabInteractable.interactionLayers.value : " + _xrGrabInteractable.interactionLayers.value);
    }
}
