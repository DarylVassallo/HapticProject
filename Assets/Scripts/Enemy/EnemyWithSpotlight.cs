using UnityEngine;
using UnityEngine.AI;

using Unity.Netcode;
using System;
using System.Collections;

//This script detects if the enemy is within the spotlight, and allows it to move if it is not (modified to use a spotlight instead of the player's camera)(modified to shake the angel when in the light, and damages it).
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class EnemyWithSpotlight : NetworkBehaviour
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
    [SerializeField] private AudioClip[] burningAudios;
    [SerializeField] private AudioClip[] footstepAudios;
    [SerializeField] private AudioClip deathAudio;
    [SerializeField] private AudioClip[] attackingAudios;
    [SerializeField] private AudioClip[] whisperingAudios;
    private AudioSource _audioSource;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    private bool _shuttingDown = false;

    [SerializeField] private int maxAttackDelay;
    private int _attackCount;

    private Animator _animator;

    [SerializeField] private float damageDelay;
    private bool canBeDamaged = true;

    private int currentAudioNum = -1;

    private EnemyManager enemyManager;

    private AudioClip _currentAudio;
    private int _currentAudioNum;

    public static event Action<GameObject> OnRemoveEnemy;

    private float damageMultiplier = 0f;

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

        enemyManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnemyManager>();
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;
        CheckpointManager.OnDestroyAllEnemies += DestroyEnemy;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;
        CheckpointManager.OnDestroyAllEnemies -= DestroyEnemy;
    }

    public override void OnNetworkDespawn()
    {
        _shuttingDown = true;
    }

    [ClientRpc]
    private void PlayAudioClientRpc()
    {
        if(_audioSource.isPlaying) return;

        _audioSource.Stop();
        _audioSource.clip = _currentAudio;
        _audioSource.Play();
        _audioSource.enabled = true; 
    }

    // private void PlayAudioClientRpc(int _audioNum, int _randomNum)
    // {
    //     AudioClip _currentAudio = _audioNum switch
    //     {
    //         0 => burningAudios[_randomNum],
    //         1 => footstepAudios[_randomNum],
    //         2 => attackingAudios[_randomNum],
    //         3 => whisperingAudios[_randomNum],
    //         4 => deathAudio,
    //     };

    //     currentAudioNum = _audioNum;
    //     if(_audioSource.isPlaying && currentAudioNum == _audioNum) return;

    //     _audioSource.Stop();
    //     _audioSource.clip = _currentAudio;
    //     _audioSource.Play();
    //     _audioSource.enabled = true; 
    // }

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
            _isInsidePCSpotLight = IsInsideSpotLight(_pcPlayerSpotLight, _pcFlashlightCharge); 
            _isInsideVRSpotLight = IsInsideSpotLight(_vrPlayerSpotLight, _vrFlashlightCharge);

            if(!_isInsidePCSpotLight && !_isInsideVRSpotLight)
            {
                if(health.isDead)
                {
                    OnRemoveEnemy?.Invoke(this.gameObject);
                    // Destroy(this.gameObject);
                }
            }
            
            // if(_isInsidePCSpotLight || _isInsideVRSpotLight)
            // {
            //     if (_isInsidePCSpotLight && _isInsideVRSpotLight)
            //     {
            //         DamageAndFreeze(2);
            //     }
            //     else if (_isInsidePCSpotLight)
            //     {
            //         DamageAndFreeze(0);
            //     }
            //     else if (_isInsideVRSpotLight)
            //     {
            //         DamageAndFreeze(1);
            //     }
            // }

            if (_isInsidePCSpotLight)
            {
                DamageAndFreeze(0);
            }

            if(!_isInsidePCSpotLight && !_isInsideVRSpotLight)
            {
                if (_isPCTooClose && _attackCount <= 0)
                {
                    StopAndAttack(damageToPlayer);  
                }
                else if(!_isPCTooClose)
                {
                    Walk();
                }
            }
        }
    }

    private void DestroyEnemy()
    {
        OnRemoveEnemy?.Invoke(this.gameObject);
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

        if (_currentAudioNum != 1 || !_audioSource.isPlaying)
        {
            _currentAudio = enemyManager.GetAppropriateAudio(1);
            PlayAudioClientRpc();
            _audioSource.volume = 0.5f;
        }
        _currentAudioNum = 1;

        _destination = _pcPlayerTransform.position;
        // _agent.destination = _destination;

        if(_agent.isOnNavMesh) _agent.SetDestination(_destination);
    }

    private void StopAndAttack(float _damage)
    {
        _attackCount = maxAttackDelay;

        _agent.speed = 0;
        
        if(!_shuttingDown)
        {
            if (_currentAudioNum != 3 || !_audioSource.isPlaying)
            {
                _currentAudio = enemyManager.GetAppropriateAudio(3);
                PlayAudioClientRpc();
            }
            _currentAudioNum = 3;
        }
        
        _agent.SetDestination(transform.position);

        _pcPlayerHealth.ChangeHealth(-_damage, -1);
    }

    private void DamageAndFreeze(int _playerType)
    {
        _agent.speed = 0;

        if(!health.isDead)
        { 
            _animator.speed = 0;

            if (_currentAudioNum != 0 || !_audioSource.isPlaying)
            {
                _currentAudio = enemyManager.GetAppropriateAudio(0);
                PlayAudioClientRpc();
            }
            _currentAudioNum = 0;
            
            _audioSource.volume = 1 - (health.GetHealth() / 100);

            // _agent.SetDestination(transform.position);

            if(canBeDamaged)
            {
                canBeDamaged = false;
                health.ChangeHealth(-damageToAngel * damageMultiplier, _playerType);
                StartCoroutine(DelayDamage());
            }
        }
        else
        {
            _animator.speed = 1;

            if (_currentAudioNum != 2)
            {
                _currentAudio = enemyManager.GetAppropriateAudio(2);
            }
            _currentAudioNum = 2;

            PlayAudioClientRpc();
            _audioSource.volume = 1;
        }

        // transform.position = new Vector3(   
        //                                 transform.position.x + 
        //                                 Mathf.Sin(shakeSpeed * Time.time * (1 - (health.GetHealth() / 100))) * shakeAmount * (1 - (health.GetHealth() / 100)),
        //                                 transform.position.y, 
        //                                 transform.position.z
        //                             );
    }

    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(damageDelay);
        canBeDamaged = true;
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

        damageMultiplier = 1 - (_spotLightDistance / (_playerFlashlightCharge.currentFlashLightRange * 0.5f));
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
