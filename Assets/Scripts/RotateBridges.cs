using UnityEngine;

public class RotateBridges : MonoBehaviour
{
    [SerializeField] private Transform firstBridges;
    [SerializeField] private Transform secondBridges;
    [SerializeField] private Transform thirdBridges;

    [SerializeField] private float rotateSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        firstBridges.Rotate(0.0f, Time.deltaTime * rotateSpeed, 0.0f, Space.Self);
        secondBridges.Rotate(0.0f, Time.deltaTime * rotateSpeed, 0.0f, Space.Self);
        thirdBridges.Rotate(0.0f, Time.deltaTime * rotateSpeed, 0.0f, Space.Self);
    }
}
