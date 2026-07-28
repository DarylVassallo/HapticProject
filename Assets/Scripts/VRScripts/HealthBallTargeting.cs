using UnityEngine;

using System.Collections;
using System;
using Unity.Netcode;

public class HealthBallTargeting : NetworkBehaviour
{
     private Vector3 _startingPosition;
    private Quaternion _startingRotation;

    private Rigidbody _rb;

    [SerializeField] private float respawnDelay;

    private bool isRespawning;  

    private Transform pcPlayer;
    private bool _canPCFunction;

    private bool _isThrown;

    private Rigidbody rb;

    [SerializeField] private float turningSpeed;
    [SerializeField] private float minDistance;
    public static event Action GivePCPlayerHealth;

    void Awake()
    {
        _startingPosition = this.transform.position;
        _startingRotation = this.transform.rotation;
        isRespawning = false;

        _canPCFunction = false;
        _isThrown = false;

        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerDataRpc;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerDataRpc;
    }
    
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void GetPCPlayerDataRpc()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
            pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        }
    }

    public void testSelectExited()
    {
        if(_canPCFunction) _isThrown = true;
    }

    private IEnumerator RespawnDelayAfterCollision()
    {
        Debug.Log("RespawnDelayAfterCollision");
        yield return new WaitForSeconds(respawnDelay);
        RespawnNow();
    }

    private void RespawnNow()
    {
        Debug.Log("RespawnNow");
        _isThrown = false;

        this.transform.position = _startingPosition;
        this.transform.rotation = _startingRotation;
                
        _rb = GetComponent<Rigidbody>();
        if(_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
        }

        isRespawning = false;
    }

    private void FixedUpdate()
    {
        if(!_isThrown) return;

        float currentDistance = (transform.position - pcPlayer.transform.position).magnitude;
        if(currentDistance <= minDistance)
        {
            GivePCPlayerHealth?.Invoke();
        }

        Vector3 currentDirection = rb.linearVelocity.normalized;
        Vector3 targetDirection = (pcPlayer.transform.position - transform.position).normalized;
        Vector3 newDirection = Vector3.RotateTowards(   currentDirection, 
                                                        targetDirection, 
                                                        turningSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 
                                                        0f).normalized;
        
        rb.linearVelocity = newDirection * rb.linearVelocity.magnitude;
    }

    private void OnTriggerStay(Collider other)
    {        
        Debug.Log("OnTriggerStay other: " + other);
        Debug.Log("OnTriggerStay other.CompareTag(HealthBowl): " + other.CompareTag("HealthBowl"));
        Debug.Log("isRespawning: " + isRespawning);

        if (!other.CompareTag("HealthBowl") && !isRespawning)
        { 
            isRespawning = true;
            StartCoroutine(RespawnDelayAfterCollision());
        }
    }
}
