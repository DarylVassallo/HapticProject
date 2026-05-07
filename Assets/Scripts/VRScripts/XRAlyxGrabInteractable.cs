using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//This script pulls interactable objects towards the VR similar to Half Life Alyx
//Source: https://www.youtube.com/watch?v=WU23Uj1oeh8
public class XRAlyxGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    [SerializeField] private float velocityThreshold = 2;
    [SerializeField] private float jumpAngleInDegree = 60;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor _nearFarInteractor;
    private Vector3 _previousPosition;
    private Rigidbody _interactableRb;
    private bool _canJump = true;

    protected override void Awake()
    {
        base.Awake();
        _interactableRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(isSelected && firstInteractorSelecting is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor && _canJump)
        {
            Transform attach = _nearFarInteractor.GetAttachTransform(this);
            Vector3 velocity = (attach.position - _previousPosition) / Time.deltaTime;
            _previousPosition = attach.position;

            if(velocity.magnitude > velocityThreshold)
            {
                Drop();
                _interactableRb.linearVelocity = ComputeVelocity();
                _canJump = false;
            }
        }
    }

    public Vector3 ComputeVelocity()
    {
        Vector3 diff = _nearFarInteractor.transform.position - transform.position;
        Vector3 diffXZ = new Vector3(diff.x, 0, diff.z);
        float diffXZLength = diffXZ.magnitude;
        float diffYLength = diff.y;

        float angleInRadian = Mathf.Clamp(diff.normalized.y * 90, jumpAngleInDegree, 90) * Mathf.Deg2Rad;

        float jumpSpeed = Mathf.Sqrt(   -Physics.gravity.y * Mathf.Pow(diffXZLength, 2) / 
                                        (   2 * 
                                            Mathf.Cos(angleInRadian) * 
                                            Mathf.Cos(angleInRadian) * 
                                            (   diffXZ.magnitude * 
                                                Mathf.Tan(angleInRadian) - 
                                                diffYLength)));

        Vector3 jumpVelocityVector = diffXZ.normalized * Mathf.Cos(angleInRadian) * jumpSpeed + Vector3.up * Mathf.Sin(angleInRadian) * jumpSpeed;

        return jumpVelocityVector;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if(args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor)
        {
            _nearFarInteractor = (UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor)args.interactorObject;
            float distance = Vector3.Distance(_nearFarInteractor.transform.position, transform.position);

            if(distance >= 0.3f)
            {
                Debug.Log("FAR: " + distance);
                trackPosition = false;
                trackRotation = false;
                throwOnDetach = false;

                
                _previousPosition = _nearFarInteractor.GetAttachTransform(this).position;
                _canJump = true;
            }else{
                Debug.Log("CLOSE: " + distance);
                trackPosition = true;
                trackRotation = true;
                throwOnDetach = true;
            }
        }
        else
        {
            trackPosition = true;
            trackRotation = true;
            throwOnDetach = true;
        }
        base.OnSelectEntered(args);
    }

    protected override void SetupRigidbodyGrab(Rigidbody rigidbody)
    {
        // base.SetupRigidbodyGrab(rigidbody);
        // base.SetupRigidbodyDrop(rigidbody);

        if (base.m_RigidbodySetupActive)
            return;

        base.m_RigidbodySetupActive = true;

        // Remember Rigidbody settings and setup to move
        base.m_WasKinematic = rigidbody.isKinematic;
        base.m_UsedGravity = rigidbody.useGravity;
        base.m_InterpolationOnGrab = rigidbody.interpolation;
#if UNITY_2023_3_OR_NEWER
        base.m_LinearDampingOnGrab = rigidbody.linearDamping;
        base.m_AngularDampingOnGrab = rigidbody.angularDamping;
#else
        base.m_LinearDampingOnGrab = rigidbody.drag;
        base.m_AngularDampingOnGrab = rigidbody.angularDrag;
#endif
        // rigidbody.isKinematic = m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous;
        // rigidbody.useGravity = false;
        // Initialize the Rigidbody to not interpolate when we drive predicted visuals.
        // See explanation in PerformVelocityVisualsUpdate().
        // if (isRigidbodyMovement && m_PredictedVisualsTransform != null)
            // rigidbody.interpolation = RigidbodyInterpolation.None;

// #if UNITY_2023_3_OR_NEWER
//         rigidbody.linearDamping = 0f;
//         rigidbody.angularDamping = 0f;
// #else
//         rigidbody.drag = 0f;
//         rigidbody.angularDrag = 0f;
// #endif
    }
}
