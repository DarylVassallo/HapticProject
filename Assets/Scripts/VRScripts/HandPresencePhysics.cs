using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
//This script allows the hand physics to match the positiona and rotation of the players real hands. Modified to alternatively go to the grab points of a held object.
//Source: https://www.youtube.com/watch?v=VG8hLKyTiJQ
public class HandPresencePhysics : MonoBehaviour
{
    [SerializeField] private Transform handTargetTransform;
    private Rigidbody _rb;

    private Transform _currentTargetTransform;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    //Modifies the physical hand's rotation and velocity to match the motion of the VR Player's hand
    void FixedUpdate()
    {
        _currentTargetTransform = handTargetTransform;

        if(Vector3.Distance(_currentTargetTransform.position, this.transform.position) >= 3f)
        {
            this.transform.position = _currentTargetTransform.position;
        }else{
            _rb.linearVelocity = (_currentTargetTransform.position - transform.position) / Time.fixedDeltaTime;

            Quaternion rotationDifference = _currentTargetTransform.rotation * Quaternion.Inverse(transform.rotation);
            rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

            Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

            _rb.angularVelocity = rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime;
        }
    }
}
