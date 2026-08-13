using UnityEngine;
using System.Collections;

public class Respawn : MonoBehaviour
{
    private Vector3 _startingPosition;
    private Quaternion _startingRotation;

    private Rigidbody _rb;

    [SerializeField] private float respawnDelay;
    private bool isRespawning;  

    void Start()
    {
        _startingPosition = this.transform.position;
        _startingRotation = this.transform.rotation;

        isRespawning = false;
    }

    //Once a small delay has passed, the ball is then teleported back to its spawn position
    private IEnumerator RespawnDelayAfterCollision()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnNow();
    }

    //This teleports the ball in its spawn position, and resets its values
    private void RespawnNow()
    {
        this.transform.position = _startingPosition;
        this.transform.rotation = _startingRotation;
                
        _rb = GetComponent<Rigidbody>();
        if(_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        isRespawning = false;

        EventsManager.ObjectRespawned(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (!other.CompareTag("RespawnPlatform") && !isRespawning)
        { 
            isRespawning = true;
            StartCoroutine(RespawnDelayAfterCollision());
        }
    }
}