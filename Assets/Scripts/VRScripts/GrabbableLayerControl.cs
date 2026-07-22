using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class GrabbableLayerControl : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _xrGrabInteractable;
    private int _standardLayer;
    private GameObject _interactor;
    private bool isRightHandInteractable = false;
    private bool isInRightHandSocket = false;

    private bool isLeftHandInteractable = false;
    private bool isInLeftHandSocket = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor leftHandSocket;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor rightHandSocket;

    [SerializeField] private bool isAttachable;
    private bool isInSocket = false;

    void Awake()
    {
        _xrGrabInteractable = this.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        _xrGrabInteractable.selectEntered.AddListener(OnGrabbed);
        _xrGrabInteractable.selectExited.AddListener(OnReleased);

        _standardLayer = this.gameObject.layer;

        // leftHandSocket = GameObject.FindWithTag("LeftHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        // leftHandSocket.selectEntered.AddListener(OnLeftHandInserted);
        // leftHandSocket.selectExited.AddListener(OnLeftHandRemoved);

        // rightHandSocket = GameObject.FindWithTag("RightHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        // rightHandSocket.selectEntered.AddListener(OnRightHandInserted);
        // rightHandSocket.selectExited.AddListener(OnRightHandRemoved);
    }

    private void OnDestroy()
    {
        if(leftHandSocket != null)
        {
            leftHandSocket.selectEntered.RemoveListener(OnLeftHandInserted);
            leftHandSocket.selectExited.RemoveListener(OnLeftHandRemoved);
        }

        if(rightHandSocket != null)
        {
            rightHandSocket.selectEntered.RemoveListener(OnRightHandInserted);
            rightHandSocket.selectExited.RemoveListener(OnRightHandRemoved);
        }
    }

    private void OnEnable()
    {
        TogglePhysicsControllers.OnChangedControllers += ChangeHandSockets;
    }

    private void OnDisable()
    {
        TogglePhysicsControllers.OnChangedControllers -= ChangeHandSockets;
    }

    private void ChangeHandSockets()
    {
        OnDestroy();

        this.gameObject.layer = _standardLayer;
        
        if(GameObject.FindWithTag("LeftHandSocket") != null)
        {
            leftHandSocket = GameObject.FindWithTag("LeftHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            leftHandSocket.selectEntered.AddListener(OnLeftHandInserted);
            leftHandSocket.selectExited.AddListener(OnLeftHandRemoved);
        }
        
        if(GameObject.FindWithTag("RightHandSocket") != null)
        {
            rightHandSocket = GameObject.FindWithTag("RightHandSocket").GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            rightHandSocket.selectEntered.AddListener(OnRightHandInserted);
            rightHandSocket.selectExited.AddListener(OnRightHandRemoved);
        }
    }

    private void OnLeftHandInserted(SelectEnterEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;
        isInLeftHandSocket = true;
        ChangeLayer(this.gameObject, -1);
    }

    private void OnLeftHandRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;
        isInLeftHandSocket = false;
        ChangeLayer(this.gameObject, -1);
    }

    private void OnRightHandInserted(SelectEnterEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;
        isInRightHandSocket = true;
        ChangeLayer(this.gameObject, -1);
    }

    private void OnRightHandRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject != _xrGrabInteractable) return;
        isInRightHandSocket = false;
        ChangeLayer(this.gameObject, -1);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Transform attachPoint = args.interactorObject.GetAttachTransform(args.interactableObject);

        _interactor = args.interactorObject.transform.gameObject;

        // _standardLayer = this.gameObject.layer;

        if (_interactor.CompareTag("LeftHandInteractor"))
        {
            isLeftHandInteractable = true;
        }else if (_interactor.CompareTag("RightHandInteractor"))
        {
            isRightHandInteractable = true;
        }

        ChangeLayer(this.gameObject, -1);
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
        if(layer == -1)
        {
            if(isLeftHandInteractable && isRightHandInteractable)
            {
                layer = LayerMask.NameToLayer("BothHandInteractable");

                ChangeInteractionLayer(0);
            }else if(isLeftHandInteractable)
            {
                layer = LayerMask.NameToLayer("LeftHandInteractable");

                ChangeInteractionLayer(-1);
            }else if(isRightHandInteractable)
            {
                layer = LayerMask.NameToLayer("RightHandInteractable");

                ChangeInteractionLayer(1);
            }
            else
            {
                if(isInLeftHandSocket)
                {
                    layer = LayerMask.NameToLayer("LeftHandInteractable");

                    ChangeInteractionLayer(-2);
                }else if(isInRightHandSocket)
                {
                    layer = LayerMask.NameToLayer("RightHandInteractable");

                    ChangeInteractionLayer(2);
                }
                else
                {
                    layer = _standardLayer;
                    ChangeInteractionLayer(0);
                }
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
        if(!isAttachable) return;

        if(interactionLayer == -2)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("InLeftHandSocket");
        }else if(interactionLayer == -1)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("LeftHandGrab");
        }else if(interactionLayer == 0)
        {
            StartCoroutine(ReturnToDefault());
        }else if(interactionLayer == 1)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("RightHandGrab");
        }else if(interactionLayer == 2)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("InRightHandSocket");
        }
    }

    IEnumerator ReturnToDefault()
    {
        yield return new WaitForSeconds(2f);

        if(!isInLeftHandSocket && !isLeftHandInteractable && !isInRightHandSocket && !isRightHandInteractable)
        {
            _xrGrabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default");
        }
    }
}