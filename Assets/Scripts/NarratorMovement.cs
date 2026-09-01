using UnityEngine;

public class NarratorMovement : MonoBehaviour
{
    [SerializeField] private Transform _ring;
    [SerializeField] private Transform _otherRing;

    [SerializeField] private Transform _targetObject;

    private float _originalYPos;
    private float _elapsed;

    private void Awake()
    {
        _originalYPos = transform.position.y;
        _elapsed = 0;
    }
    private void FixedUpdate()
    {        
        transform.LookAt(_targetObject);

        Debug.Log("_originalYPos: " + _originalYPos);
       

        _elapsed += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, _originalYPos + Mathf.Sin(_elapsed), transform.position.z);

        Debug.Log("Mathf.Sin(" + _elapsed + "): " + Mathf.Sin(_elapsed));
        Debug.Log("transform.position: " + transform.position);

        RotateRings();
    }

    private void RotateRings()
    {
        _ring.Rotate(Vector3.forward * 100f * Time.deltaTime);
        _otherRing.Rotate(Vector3.forward * -100f * Time.deltaTime);
    }
}
