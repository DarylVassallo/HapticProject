using UnityEngine;
using System.Collections;

public class Respawn : MonoBehaviour
{
    private Vector3 _startingPosition;
    private Quaternion _startingRotation;

    private Rigidbody _rb;

    [SerializeField] private float respawnDelay;

    void Start()
    {
        _startingPosition = this.transform.position;
        _startingRotation = this.transform.rotation;
    }

    //After a delay, the object is reset to it's initial position and rotation
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        this.transform.position = _startingPosition;
        this.transform.rotation = _startingRotation;
                
        _rb = this.GetComponent<Rigidbody>();
        if(_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
        }
    }
}