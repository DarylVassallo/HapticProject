using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor near;

    private Transform currentTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (near.firstGrabTransform != null && near.hasAttachment && near.hasGrabbedFar == false)
        {
            currentTarget = near.firstGrabTransform;
        }else{
            currentTarget = target;
        }
        rb.linearVelocity = (currentTarget.position - transform.position) / Time.fixedDeltaTime;

        Quaternion rotationDifference = currentTarget.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}
