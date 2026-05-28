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
        _startingPosition = this.transform.parent.transform.position;
        _startingRotation = this.transform.parent.transform.rotation;
    }

    public void ActivateRespawn()
    {
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        this.transform.parent.transform.position = _startingPosition;
        this.transform.parent.transform.rotation = _startingRotation;
                
        _rb = this.transform.parent.GetComponent<Rigidbody>();
        if(_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
        }
    }
}
