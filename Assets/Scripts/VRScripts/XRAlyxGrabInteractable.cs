using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//This script pulls interactable objects towards the VR similar to Half Life Alyx
//Source: https://www.youtube.com/watch?v=WU23Uj1oeh8
public class XRAlyxGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    public float velocityThreshold = 2;
    public float jumpAngleInDegree = 60;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor rayInteractor;
    private Vector3 previousPos;
    private Rigidbody interactableRigidbody;
    private bool canJump = true;

    protected override void Awake()
    {
        base.Awake();
        interactableRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(isSelected && firstInteractorSelecting is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor && canJump)
        {
            Vector3 velocity = (rayInteractor.transform.position - previousPos) / Time.deltaTime;
            previousPos = rayInteractor.transform.position;

            if(velocity.magnitude > velocityThreshold)
            {
                Drop();
                interactableRigidbody.linearVelocity = ComputeVelocity();
                canJump = false;
            }
        }
    }

    public Vector3 ComputeVelocity()
    {
        Vector3 diff = rayInteractor.transform.position - transform.position;
        Vector3 diffXZ = new Vector3(diff.x, 0, diff.z);
        float diffXZLength = diffXZ.magnitude;
        float diffYLength = diff.y;

        float angleInRadian = jumpAngleInDegree * Mathf.Deg2Rad;

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
            rayInteractor = (UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor)args.interactorObject;
            float distance = Vector3.Distance(rayInteractor.transform.position, transform.position);

            if(distance >= 0.3)
            {
                trackPosition = false;
                trackRotation = false;
                throwOnDetach = false;

                
                previousPos = rayInteractor.transform.position;
                canJump = true;
            }else{
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
