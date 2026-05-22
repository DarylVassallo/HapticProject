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
        _interactor = args.interactorObject.transform.gameObject;

        _standardLayer = this.gameObject.layer;
        if (_interactor.CompareTag("LeftHandInteractor"))
        {
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
                layer = 0;
            }
        }

        currentObject.layer = layer;

        foreach (Transform child in currentObject.transform)
        {
            ChangeLayer(child.gameObject, layer);
        }
    }
}
