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
    private FlashlightCharge _pcFlashlightCharge;
    private FlashlightCharge _vrFlashlightCharge;

    private Transform _pcPlayerSpotLight;
    private bool _isPCTooClose;
    private bool _isPCTooFar;
    private bool _isInsidePCSpotLight;

    private Transform _vrPlayerSpotLight;
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
    [SerializeField] private AudioClip[] attackingAudios;
    [SerializeField] private AudioClip[] whisperingAudios;
    private AudioSource _audioSource;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    private bool _shuttingDown = false;

    [SerializeField] private int maxAttackDelay;
    private int _attackCount;

    private Animator _animator;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        health = this.gameObject.GetComponent<Health>();
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _animator = this.gameObject.transform.GetChild(0).GetComponent<Animator>();

        GetPCPlayerData();
        
        _canPCFunction = true;
        _canVRFunction = true;

        _attackCount = maxAttackDelay;
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

    public override void OnNetworkDespawn()
    {
        _shuttingDown = true;
    }

    [ClientRpc]
    private void PlayAudioClientRpc(int _audioNum)
    {
        AudioClip _currentAudio = burningAudio;
        switch (_audioNum)
        {
            case 0:
                _currentAudio = burningAudio;
                break;
            case 1:
                _currentAudio = footstepAudio;
                break;
            case 2:
                _currentAudio = attackingAudios[Random.Range(0, attackingAudios.Length)];
                break;
            case 3:
                _currentAudio = whisperingAudios[Random.Range(0, whisperingAudios.Length)];
                break;
        }

        if(!_audioSource.isPlaying || _audioSource.clip != _currentAudio)
        {
            _audioSource.Stop();
            _audioSource.clip = _currentAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 
        }
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
            _pcPlayerHealth = _pcPlayerTransform.GetComponent<Health>();
            _pcPlayerSpotLight = GameObject.FindGameObjectWithTag("PCFlashLight").transform;
            _pcFlashlightCharge = _pcPlayerSpotLight.GetComponent<FlashlightCharge>();
            
            _vrPlayerSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").transform;
            _vrFlashlightCharge = _vrPlayerSpotLight.GetComponent<FlashlightCharge>();
            
            _canPCFunction = true;
        }
    }

    private void GetVRPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("VRPlayer") != null)
        {
            // _vrPlayerSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();
            _canVRFunction = true;
        }
    }

    void FixedUpdate()
    {
        if(!_canPCFunction) GetPCPlayerData();
        if(!_canVRFunction) GetVRPlayerData();

        if(!IsServer) return;

        // if(!_canPCFunction && !_canVRFunction) return;

        if(_attackCount > 0) _attackCount--;

        (_isPCTooClose, _isPCTooFar) = IsCloseToPlayer(_pcPlayerTransform);

        if (_isPCTooFar)
        {
            StandBy();
        }else{

            if (_isPCTooClose && _attackCount <= 0) StopAndAttack(damageToPlayer);        

            _isInsidePCSpotLight = IsInsideSpotLight(_pcPlayerSpotLight, _pcFlashlightCharge); 
            _isInsideVRSpotLight = IsInsideSpotLight(_vrPlayerSpotLight, _vrFlashlightCharge);

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
        _animator.speed = 1;

        PlayAudioClientRpc(1);
        _audioSource.volume = 1;

        _destination = _pcPlayerTransform.position;
        // _agent.destination = _destination;
        _agent.SetDestination(_destination);
    }

    private void StopAndAttack(float _damage)
    {
        _attackCount = maxAttackDelay;

        _agent.speed = 0;
        
        if(!_shuttingDown) PlayAudioClientRpc(2);
        
        _agent.SetDestination(transform.position);

        _pcPlayerHealth.ChangeHealth(-_damage, -1);
    }

    private void DamageAndFreeze(int _playerType)
    {
        _agent.speed = 0;

        if(!health.isDead) _animator.speed = 0;

        PlayAudioClientRpc(0);

        _audioSource.volume = 1 - (health.GetHealth() / 100);

        _agent.SetDestination(transform.position);

        health.ChangeHealth(-damageToAngel, _playerType);

        // transform.position = new Vector3(   
        //                                 transform.position.x + 
        //                                 Mathf.Sin(shakeSpeed * Time.time * (1 - (health.GetHealth() / 100))) * shakeAmount * (1 - (health.GetHealth() / 100)),
        //                                 transform.position.y, 
        //                                 transform.position.z
        //                             );
    }
    
    //Used ChatGPT here
    bool IsInsideSpotLight(Transform _playerSpotLight, FlashlightCharge _playerFlashlightCharge)
    {
        if (_playerSpotLight == null) return false;
        if (!_playerSpotLight.gameObject.activeSelf) return false;

        _positionDifference = transform.position - _playerSpotLight.position;
        _spotLightDistance = _positionDifference.magnitude;

        if (_spotLightDistance > (_playerFlashlightCharge.currentFlashLightRange * 0.5f))
        {
            return false;
        }

        _spotLightAngle = Vector3.Angle(_playerSpotLight.forward, _positionDifference);

        if (_spotLightAngle > 55 * 0.5f)
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
