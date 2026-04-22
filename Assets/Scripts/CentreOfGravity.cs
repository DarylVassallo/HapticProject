using UnityEngine;
//This script shows the centre of gravity of an object (good for imbalanced items such as a large hammer).
//Source: https://www.youtube.com/watch?v=pu50eTSlvdk
public class CentreOfGravity : MonoBehaviour
{
    [SerializeField] private Vector3 _centreOfMass;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = _centreOfMass;    
    }

    void Update()
    {
        #if UNITY_EDITOR
        _rb.centerOfMass = _centreOfMass;
        _rb.WakeUp();
        #endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.rotation * _centreOfMass, 0.05f);
    }
}
