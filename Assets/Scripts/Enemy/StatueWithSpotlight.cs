using UnityEngine;
using UnityEngine.AI;

using Unity.Netcode;

//This script detects if the statue is within the spotlight, and allows it to move if it is not (modified to use a spotlight instead of the player's camera)(modified to shake the angel when in the light, and damages it).
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class StatueWithSpotlight : NetworkBehaviour
{
    private NavMeshAgent _agent;

    private Transform _pcPlayerTransform;
    private Health _pcPlayerHealth;
    private Light _pcPlayerSpotLight;
    private bool _isPCTooClose;
    private bool _isPCTooFar;
    private bool _isInsidePCSpotLight;

    private Light _vrPlayerSpotLight;
    private bool _isInsideVRSpotLight;

    private float _spotLightDistance;
    private float _spotLightAngle;
    
    private Vector3 _destination;
    
    [SerializeField] private float agentSpeed;

    private Vector3 _positionDifference;

    [Header("Shake")]
    [SerializeField] private float shakeSpeed;
    [SerializeField] private float shakeAmount;


    [Header("Damage")]
    [SerializeField] private float damageToAngel;
    [SerializeField] private float damageToPlayer;
    [SerializeField] private float _tooCloseDistance;
    [SerializeField] private float _tooFarDistance;
     private Health health;


    [Header("Audio")]
    [SerializeField] private AudioClip burningAudio;
    [SerializeField] private AudioClip footstepAudio;
    [SerializeField] private AudioClip[] whisperingAudios;
    private AudioSource _audioSource;

    private bool canPCFunction = false;
    private bool canVRFunction = false;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        health = this.gameObject.GetComponent<Health>();
        _audioSource = this.gameObject.GetComponent<AudioSource>();
        // Debug.Log(this.gameObject + " : Awake");

        _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        _pcPlayerHealth = _pcPlayerTransform.GetComponent<Health>();
        _pcPlayerSpotLight = _pcPlayerTransform.GetComponentInChildren<Light>();
        
        _vrPlayerSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();
        
        canPCFunction = true;
        canVRFunction = true;
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            // Debug.Log(this.gameObject + " : GetPCPlayerData");
            _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
            _pcPlayerHealth = _pcPlayerTransform.GetComponent<Health>();
            _pcPlayerSpotLight = _pcPlayerTransform.GetComponentInChildren<Light>();
            
            _vrPlayerSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();
            
            canPCFunction = true;
        }
    }

    private void GetVRPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("VRPlayer") != null)
        {
            // Debug.Log(this.gameObject + " : GetVRPlayerData");
            // _vrPlayerSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();
            canVRFunction = true;
        }
    }

    void FixedUpdate()
    {
        Debug.Log("canPCFunction: " + canPCFunction);
        Debug.Log("canVRFunction: " + canVRFunction);
        if(!canPCFunction) GetPCPlayerData();
        if(!canVRFunction) GetVRPlayerData();

        if(!IsServer) return;

        // Debug.Log("Angel 1");
        if(!canPCFunction && !canVRFunction) return;

        (_isPCTooClose, _isPCTooFar) = IsCloseToPlayer(_pcPlayerTransform);
        Debug.Log("_isPCTooClose: " + _isPCTooClose);
        Debug.Log("_isPCTooFar: " + _isPCTooFar);
        if (_isPCTooFar)
        {
            StandBy();
        }else{

            if (_isPCTooClose) StopAndAttack(damageToPlayer);        

            _isInsidePCSpotLight = IsInsideSpotLight(_pcPlayerSpotLight); 
            _isInsideVRSpotLight = IsInsideSpotLight(_vrPlayerSpotLight);

            if (_isInsidePCSpotLight && _isInsideVRSpotLight)
            {
                DamageAndFreeze(2);
            }
            else if (_isInsidePCSpotLight)
            {
                DamageAndFreeze(0);
            }
            else if (_isInsideVRSpotLight)
            {
                DamageAndFreeze(1);
            }
            else if(!_isPCTooClose)
            {
                Walk();
            }
        }
    }

    private void StandBy()
    {
        _agent.speed = 0;
        _audioSource.enabled = false;
        _agent.SetDestination(transform.position);
    }
    private void Walk()
    {
        _agent.speed = agentSpeed;

        SwapAudio(footstepAudio);
        _audioSource.volume = 1;

        _destination = _pcPlayerTransform.position;
        // _agent.destination = _destination;
        _agent.SetDestination(_destination);
    }

    private void StopAndAttack(float _damage)
    {
        _agent.speed = 0;
        _audioSource.enabled = false;
        
        _agent.SetDestination(transform.position);

        _pcPlayerHealth.ChangeHealth(-_damage, -1);
    }

    private void DamageAndFreeze(int _playerType)
    {
        _agent.speed = 0;

        SwapAudio(burningAudio);

        _audioSource.volume = 1 - (health.GetHealth() / 100);

        _agent.SetDestination(transform.position);

        health.ChangeHealth(-damageToAngel, _playerType);
        transform.position = new Vector3(   
                                        transform.position.x + 
                                        Mathf.Sin(shakeSpeed * Time.time * (1 - (health.GetHealth() / 100))) * shakeAmount * (1 - (health.GetHealth() / 100)),
                                        transform.position.y, 
                                        transform.position.z
                                    );
    }

    private void SwapAudio(AudioClip _audioClip)
    {
        if(_audioSource.clip != _audioClip)
        {
            _audioSource.Stop();
            _audioSource.clip = _audioClip;
            _audioSource.Play();
        }
        _audioSource.enabled = true;
    }
    
    //Used ChatGPT here
    bool IsInsideSpotLight(Light _playerSpotLight)
    {
        Debug.Log("_playerSpotLight: " + _playerSpotLight);
        if (_playerSpotLight == null) return false;
        if (!_playerSpotLight.enabled) return false;

        _positionDifference = transform.position - _playerSpotLight.transform.position;
        _spotLightDistance = _positionDifference.magnitude;
        
        Debug.Log("_spotLightDistance: " + _spotLightDistance);
        Debug.Log("(_playerSpotLight.range * 0.5f): " + (_playerSpotLight.range * 0.5f));
        if (_spotLightDistance > (_playerSpotLight.range * 0.5f))
        {
            return false;
        }

        _spotLightAngle = Vector3.Angle(_playerSpotLight.transform.forward, _positionDifference);
        
        Debug.Log("_spotLightAngle: " + _spotLightAngle);
        Debug.Log("(_playerSpotLight.spotAngle * 0.5f): " + (_playerSpotLight.spotAngle * 0.5f));
        if (_spotLightAngle > _playerSpotLight.spotAngle * 0.5f)
        {
            return false;
        }

        return true;
    }

    (bool _isTooClose, bool _isTooFar) IsCloseToPlayer(Transform _playerTransform)
    {
        float distance = (transform.position - _playerTransform.position).magnitude;
        if (distance <= _tooCloseDistance) return (true, false);
        if(distance > _tooFarDistance)  return (false, true);
        return (false, false);
    }
}
