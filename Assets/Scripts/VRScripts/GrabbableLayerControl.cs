using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableLayerControl : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _xrGrabInteractable;
    private int _standardLayer;
    private GameObject _interactor;
    public bool isRightHandInteractable = false;
    public bool isLeftHandInteractable = false;

    void Awake()
    {
        _xrGrabInteractable = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _xrGrabInteractable.selectEntered.AddListener(OnGrabbed);
        _xrGrabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log(this.gameObject + " : OnGrabbed");

        _interactor = args.interactorObject.transform.gameObject;

        _standardLayer = this.gameObject.layer;

        Debug.Log(this.gameObject + " : _interactor : " + _interactor);

        if (_interactor.CompareTag("LeftHandInteractor"))
        {
            Debug.Log(this.gameObject + " : LeftHandInteractor");

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
            Debug.Log(this.gameObject + " : RightHandInteractor");

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
        Debug.Log(this.gameObject + " : ChangeLayer");
        Debug.Log(this.gameObject + " : currentObject : " + currentObject);
        Debug.Log(this.gameObject + " : layer : " + layer);

        if(layer == -1)
        {
            if(isLeftHandInteractable && isRightHandInteractable)
            {
                ChangeInteractionLayer(0);
                layer = LayerMask.NameToLayer("BothHandInteractable");
            }else if(isLeftHandInteractable)
            {
                ChangeInteractionLayer(-1);
                layer = LayerMask.NameToLayer("LeftHandInteractable");
            }else if(isRightHandInteractable)
            {
                ChangeInteractionLayer(1);
                layer = LayerMask.NameToLayer("RightHandInteractable");
            }
            else
            {
                layer = _standardLayer;
            }
        }

        Debug.Log(this.gameObject + " : new layer : " + layer);

        currentObject.layer = layer;

        foreach (Transform child in currentObject.transform)
        {
            ChangeLayer(child.gameObject, layer);
        }
    }

    public void ChangeInteractionLayer(int interactionLayer)
    {
        Debug.Log(this.gameObject + " : ChangeLayer");
        Debug.Log(this.gameObject + " : this.gameObject : " + this.gameObject);
        Debug.Log(this.gameObject + " : interactionLayer : " + interactionLayer);

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
    }
}
