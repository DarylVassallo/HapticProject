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
    private bool _isSelected;
    private bool _isEmitting;

    private Rigidbody rb;

    private Vector3 newDirection;
    private float currentDistance;
    [SerializeField] private float minDistance;
    public static event Action GivePCPlayerHealth;

    [SerializeField] private Material pcPlayerPipes;
    private Color baseColor = Color.orange;
    private float intensity;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;
    [SerializeField] private float intensityIncrement;

    void Awake()
    {
        _startingPosition = this.transform.position;
        _startingRotation = this.transform.rotation;
        isRespawning = false;

        _canPCFunction = false;

        _isThrown = false;

        _isSelected = false;
        _isEmitting = false;
        intensity = minIntensity;

        rb = GetComponent<Rigidbody>();

        pcPlayerPipes.EnableKeyword("_EMISSION");
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

    public void grabbedHealthBall()
    {
        Debug.Log("grabbedHealthBall: " + _canPCFunction);
        if(_canPCFunction) _isSelected = true;
    }
    
    public void releasedHealthBall()
    {
        Debug.Log("releasedHealthBall: " + _canPCFunction);
        if(_canPCFunction)
        {
            _isThrown = true;
            _isSelected = false;

            intensity = 0;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
            pcPlayerPipes.SetColor("_EmissionColor", finalColor);
        }
    }

    private IEnumerator RespawnDelayAfterCollision()
    {
        yield return new WaitForSeconds(respawnDelay);
        RespawnNow();
    }

    private void RespawnNow()
    {
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
        if(!_isThrown && !_isSelected) return;

        if(_isSelected) Selected();
        if(_isThrown) Thrown();
    }

    private void Selected()
    {
        if(_isEmitting)
        {
            intensity -= intensityIncrement;
            if(intensity < minIntensity)
            {
                intensity = minIntensity;
                _isEmitting = false;
            }
        }
        else
        {
            intensity += intensityIncrement;
            if(intensity > maxIntensity)
            {
                intensity = maxIntensity;
                _isEmitting = true;
            }
        }

        Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
        pcPlayerPipes.SetColor("_EmissionColor", finalColor);
    }

    private void Thrown()
    {
        currentDistance = (transform.position - pcPlayer.transform.position).magnitude;
        if(currentDistance <= minDistance)
        {
            
            currentDistance *= 3;
        }
        currentDistance = (70 - currentDistance) * 1.1f;
        if(currentDistance < 0) currentDistance = 0;

        newDirection = Vector3.RotateTowards(   rb.linearVelocity.normalized, 
                                                (pcPlayer.transform.position - transform.position).normalized, 
                                                currentDistance * Mathf.Deg2Rad * Time.fixedDeltaTime, 
                                                0f).normalized;
        
        rb.linearVelocity = newDirection * rb.linearVelocity.magnitude;
    }

    private void OnTriggerEnter(Collider other)
    {        
        Debug.Log("other: " + other);
        Debug.Log("other.tag: " + other.tag);

        if (other.CompareTag("PCPlayer"))
        { 
            GivePCPlayerHealth?.Invoke();
            RespawnNow();
        }

        if (!other.CompareTag("HealthBowl") && !isRespawning)
        { 
            isRespawning = true;
            StartCoroutine(RespawnDelayAfterCollision());
        }
    }
}
