using UnityEngine;

public class CentreOfGravity : MonoBehaviour
{
    public Vector3 centreOfMass;
    private Rigidbody _rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = centreOfMass;    
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        _rb.centerOfMass = centreOfMass;
        _rb.WakeUp();
        #endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.rotation * centreOfMass, 0.05f);
    }
}
