using UnityEngine;

using System.Collections;
using System;
using Unity.Netcode;

public class HealthBallTargeting : NetworkBehaviour
{
    private Rigidbody _rb;

    [SerializeField] private float respawnDelay;

    // private bool isRespawning;  

    private Transform pcPlayer;
    private bool _canPCFunction;

    private bool _isThrown;
    private bool _isSelected;
    private bool _isEmitting;

    private Rigidbody rb;

    private Vector3 newDirection;
    private float currentDistance;
    [SerializeField] private float minDistance;

    [SerializeField] private Material pcPlayerPipes;
    private Color baseColor = Color.orange;
    private float intensity;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;
    [SerializeField] private float intensityIncrement;

    void Awake()
    {
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
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;
        EventsManager.OnObjectRespawned += Respawned;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;
        EventsManager.OnObjectRespawned -= Respawned;
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetPCPlayerBodyDataRpc()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
            pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        }
    }

    //This detects if the ball has been grabbed by the VR Player
    public void grabbedHealthBall()
    {
        if(_canPCFunction) _isSelected = true;
    }
    
    //This detects if the ball has been released by the VR Player
    public void releasedHealthBall()
    {
        if(_canPCFunction)
        {
            _isThrown = true;
            _isSelected = false;

            intensity = 0;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
            pcPlayerPipes.SetColor("_EmissionColor", finalColor);
        }
    }

    private void Respawned(GameObject entity)
    {
        if(entity == this.gameObject) _isThrown = false;
    }

    private void FixedUpdate()
    {
        if(!_isThrown && !_isSelected) return;

        if(_isSelected) Selected();
        if(_isThrown) Thrown();
    }

    //Once grabbed, this causes the pipes on the PC Player to pulse lighting, 
    // with them slowly glowing brighter and darker
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

    //Once thrown, the ball will change its direction of motion slowly towards the PC Player, 
    // and therefore seemingly naturally hit them
    private void Thrown()
    {
        currentDistance = (transform.position - pcPlayer.transform.position).magnitude;
        
        //If the ball is close to the PC Player, it will increase the intensity of the directional pull
        if(currentDistance <= minDistance) currentDistance *= 3;

        currentDistance = (70 - currentDistance) * 1.1f;
        if(currentDistance < 0) currentDistance = 0;

        newDirection = Vector3.RotateTowards(   rb.linearVelocity.normalized, 
                                                (pcPlayer.transform.position - transform.position).normalized, 
                                                currentDistance * Mathf.Deg2Rad * Time.fixedDeltaTime, 
                                                0f).normalized;
        
        rb.linearVelocity = newDirection * rb.linearVelocity.magnitude;
    }

    //If the ball collides with an object, the ball will be reset back to their spawn point. 
    // In addition, if the collided object was the PC Player, it will trigger them to gain health.
    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("PCPlayer"))
        { 
            EventsManager.GivePCPlayerHealth();
        }
    }
}
