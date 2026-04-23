using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//This script allows the hand physics to match the positiona and rotation of the players real hands. Modified to alternatively go to the grab points of a held object.
//Source: https://www.youtube.com/watch?v=VG8hLKyTiJQ
public class HandPresencePhysics : MonoBehaviour
{
    [SerializeField] private Transform handTargetTransform;
    private Rigidbody _rb;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor _nearFarInteractor;

    private Transform _currentTargetTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_nearFarInteractor.firstGrabTransform != null && _nearFarInteractor.hasAttachment && _nearFarInteractor.hasGrabbedFar == false)
        {
            _currentTargetTransform = _nearFarInteractor.firstGrabTransform;
        }else{
            _currentTargetTransform = handTargetTransform;
        }
        _rb.linearVelocity = (_currentTargetTransform.position - transform.position) / Time.fixedDeltaTime;

        Quaternion rotationDifference = _currentTargetTransform.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        _rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}
