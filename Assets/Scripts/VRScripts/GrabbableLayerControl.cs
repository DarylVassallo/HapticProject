using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableLayerControl : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _xrGrabInteractable;
    private int _standardLayer;
    private GameObject _interactor;
    private bool _isRightHandInteractable = false;
    private bool _isLeftHandInteractable = false;

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

            _isLeftHandInteractable = true;

            if(_isLeftHandInteractable && _isRightHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }else if(_isLeftHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }
        }else if (_interactor.CompareTag("RightHandInteractor"))
        {
            Debug.Log(this.gameObject + " : RightHandInteractor");

            _isRightHandInteractable = true;

            if(_isLeftHandInteractable && _isRightHandInteractable)
            {
                ChangeLayer(this.gameObject, -1);
            }else if(_isRightHandInteractable)
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
            _isLeftHandInteractable = false;             
        }else if (_interactor.CompareTag("RightHandInteractor"))
        {
            _isRightHandInteractable = false;
        }

        ChangeLayer(this.gameObject, -1);
    }

    private void ChangeLayer(GameObject currentObject, int layer)
    {
        Debug.Log(this.gameObject + " : ChangeLayer");
        Debug.Log(this.gameObject + " : currentObject : " + currentObject);
        Debug.Log(this.gameObject + " : layer : " + layer);

        if(layer == -1)
        {
            if(_isLeftHandInteractable && _isRightHandInteractable)
            {
                layer = LayerMask.NameToLayer("BothHandInteractable");
            }else if(_isLeftHandInteractable)
            {
                layer = LayerMask.NameToLayer("LeftHandInteractable");
            }else if(_isRightHandInteractable)
            {
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
}
