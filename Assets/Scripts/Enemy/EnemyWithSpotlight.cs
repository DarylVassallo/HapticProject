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

    private GameObject _pcPlayer;
    private Health _pcPlayerHealth;

    private Transform _pcPlayerSpotLight;
    private bool _isPCTooClose;
    private bool _isPCTooFar;
    private bool _isInsidePCSpotLight;

    private float _spotLightDistance;
    private float _spotLightAngle;
    
    private Vector3 _destination;
    
    [SerializeField] private float agentSpeed;

    private Vector3 _positionDifference;

    [Header("Damage")]
    [SerializeField] private float damageToAngel;
    [SerializeField] private float damageToPlayer;
    [SerializeField] private float _tooCloseDistance;
    [SerializeField] private float _tooFarDistance;

    private bool _notifiedOfDeath;
    private float _notifiedHealth;
    private float _notifiedFlashlightRange;

    [Header("Audio")]
    [SerializeField] private AudioClip[] burningAudios;
    [SerializeField] private AudioClip[] footstepAudios;
    [SerializeField] private AudioClip deathAudio;
    [SerializeField] private AudioClip[] attackingAudios;
    [SerializeField] private AudioClip[] whisperingAudios;
    private AudioSource _audioSource;

    private bool _shuttingDown = false;

    [SerializeField] private int maxAttackDelay;
    private int _attackCount;

    private Animator _animator;

    [SerializeField] private float damageDelay;
    private bool _canBeDamaged;

    private EnemyManager _enemyManager;

    private AudioClip _currentAudio;
    private int _currentAudioNum;

    private float _damageMultiplier;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _animator = this.gameObject.transform.GetChild(0).GetComponent<Animator>();

        GetPCPlayerData();

        _attackCount = maxAttackDelay;

        _enemyManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnemyManager>();

        _canBeDamaged = true;

        _notifiedOfDeath = false;
        _notifiedHealth = 100f;
        _notifiedFlashlightRange = 0;

        _damageMultiplier = 0f;
    }

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerData;
        EventsManager.OnDestroyAllEnemies += DestroyEnemy;

        EventsManager.OnEntityKilled += RecieveDeath;
        EventsManager.OnEntityChangedHealth += RecieveChangedHealth;
        EventsManager.OnEntityChangedFlashlightRange += RecieveFlashlightRange;

        EventsManager.OnSendAppropriateEnemyAudio += PlayAudio;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerData;
        EventsManager.OnDestroyAllEnemies -= DestroyEnemy;

        EventsManager.OnEntityKilled -= RecieveDeath;
        EventsManager.OnEntityChangedHealth -= RecieveChangedHealth;
        EventsManager.OnEntityChangedFlashlightRange -= RecieveFlashlightRange;

        EventsManager.OnSendAppropriateEnemyAudio -= PlayAudio;
    }

    public override void OnNetworkDespawn()
    {
        _shuttingDown = true;
    }

    [ClientRpc]
    private void PlayWalkAudioClientRpc()
    {
        EventsManager.GetAppropriateEnemyAudio(this.gameObject, 1);
    }

    [ClientRpc]
    private void PlayAttackAudioClientRpc()
    {
        EventsManager.GetAppropriateEnemyAudio(this.gameObject, 3);
    }

    [ClientRpc]
    private void PlayDamageAudioClientRpc()
    {
        EventsManager.GetAppropriateEnemyAudio(this.gameObject, 0);
    }

    [ClientRpc]
    private void PlayDeathAudioClientRpc()
    {
        EventsManager.GetAppropriateEnemyAudio(this.gameObject, 2);
    }

    private void PlayAudio(GameObject enemy, AudioClip _newAudio)
    {
        if(_audioSource.isPlaying || enemy != this.gameObject) return;

        _audioSource.Stop();
        _audioSource.clip = _newAudio;
        _audioSource.Play();
        _audioSource.enabled = true; 
    }

    [ClientRpc]
    public void RemoveEnemyClientRpc()
    {
        EventsManager.RemoveEnemy(this.gameObject);
    }

    [ClientRpc]
    public void DisableAudioSourceClientRpc()
    {
        _audioSource.enabled = false;
    }

    [ClientRpc]
    public void MoveAnimationClientRpc()
    {
        _animator.speed = 1;
    }

    [ClientRpc]
    public void FreezeAnimationClientRpc()
    {
        _animator.speed = 0;
    }

    [ClientRpc]
    public void WalkAudioVolumeClientRpc()
    {
        _audioSource.volume = 0.5f;
    }

    [ClientRpc]
    public void DamageAudioClientRpc()
    {
        _audioSource.volume = 1 - (_notifiedHealth / 100);
    }

    [ClientRpc]
    public void DeathAudioVolumeClientRpc()
    {
        _audioSource.volume = 1;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer");
            _pcPlayerSpotLight = GameObject.FindGameObjectWithTag("PCFlashLight").transform;
        }
    }

    void FixedUpdate()
    {
        if(!IsOwner) return;

        if(_attackCount > 0) _attackCount--;

        (_isPCTooClose, _isPCTooFar) = IsCloseToPlayer(_pcPlayer.transform);

        //The enemy does not move if it is too far from the PC Player
        if (_isPCTooFar)
        {
            StandBy();
        }else{    
            _isInsidePCSpotLight = IsInsideSpotLight(_pcPlayerSpotLight); 

            //Freezes and damages the enemy if it is within the PC Player's flashlight's light
            if (_isInsidePCSpotLight)
            {
                DamageAndFreeze();
            }
            else
            {
                //Removes the enemy if it is dead
                if(_notifiedOfDeath)
                {
                    RemoveEnemyClientRpc();

                //The enemy stops and attacks the player if it is close enough to the PC Player,
                //  and not within their light
                }else if (_isPCTooClose && _attackCount <= 0)
                {
                    StopAndAttack(damageToPlayer);  
                
                //The enemy walks towards the  PC Player if it is not too far from them, 
                // and not within their light
                }else if(!_isPCTooClose)
                {
                    Walk();
                }
            }
        }
    }

    private void RecieveDeath(GameObject entity)
    {
        if(entity == this.gameObject) _notifiedOfDeath = true;
    }

    private void RecieveChangedHealth(GameObject entity, float newHealth)
    {
        if(entity == this.gameObject) _notifiedHealth = newHealth;
    }
    
    private void RecieveFlashlightRange(float newFlashlightRange)
    {
        _notifiedFlashlightRange = newFlashlightRange;
    }

    // This triggers the funciton to properly remove the enemy
    private void DestroyEnemy()
    {
        RemoveEnemyClientRpc();
    }

    //This stops the enemy from moving
    private void StandBy()
    {
        _agent.speed = 0;
        DisableAudioSourceClientRpc();
        _agent.SetDestination(transform.position);
    }

    //This causes the enemy to move towards the PC Player
    private void Walk()
    {
        _agent.speed = agentSpeed;
        _agent.isStopped = false;

        if(_animator.speed != 1) MoveAnimationClientRpc();

        if (_currentAudioNum != 1 || !_audioSource.isPlaying)
        {
            PlayWalkAudioClientRpc();
            WalkAudioVolumeClientRpc();
        }
        _currentAudioNum = 1;

        _destination = _pcPlayer.transform.position;

        if(_agent.isOnNavMesh) _agent.SetDestination(_destination);
    }

    //This stops the enemy, and reduces the PC Player's health
    private void StopAndAttack(float _damage)
    {
        _attackCount = maxAttackDelay;

        _agent.speed = 0;
        
        if(!_shuttingDown)
        {
            if (_currentAudioNum != 3 || !_audioSource.isPlaying) PlayAttackAudioClientRpc();
            _currentAudioNum = 3;
        }
        
        _agent.SetDestination(transform.position);

        EventsManager.ChangeHealthForEntity(_pcPlayer, -_damage);
    }

    //This stops the enemy, and reduces the enemy's health
    private void DamageAndFreeze()
    {
        _agent.speed = 0;

        //Reduces the enemy's health if it is not dead
        if(!_notifiedOfDeath)
        {             
            if(_animator.speed != 0) FreezeAnimationClientRpc();

            if (_currentAudioNum != 0 || !_audioSource.isPlaying) PlayDamageAudioClientRpc();
            _currentAudioNum = 0;
            
            if(IsServer) DamageAudioClientRpc();

            //This damages to the enemy in increments
            if(_canBeDamaged)
            {
                _canBeDamaged = false;
                EventsManager.ChangeHealthForEntity(this.gameObject, -damageToAngel * _damageMultiplier);
                StartCoroutine(DelayDamage());
            }

        //Properly removes the enemy if it has no more health
        } else {
            if(_animator.speed != 1) MoveAnimationClientRpc();

            _currentAudioNum = 2;

            PlayDeathAudioClientRpc();
            DeathAudioVolumeClientRpc();
        }
    }

    //After a delay, the enemy is able to be damaged again by the PC Player's light
    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(damageDelay);
        _canBeDamaged = true;
    }
    
    //This checks if the enemy is within the PC Player's flashlights range and angle (which depends on its strength)
    //Used ChatGPT here
    bool IsInsideSpotLight(Transform _playerSpotLight)
    {
        if (_playerSpotLight == null) return false;
        if (!_playerSpotLight.gameObject.activeSelf) return false;

        _positionDifference = transform.position - _playerSpotLight.position;
        _spotLightDistance = _positionDifference.magnitude;

        //Returns false if the enemy is outside of the flashlight's range
        if (_spotLightDistance > (_notifiedFlashlightRange * 0.5f)) return false;

        _spotLightAngle = Vector3.Angle(_playerSpotLight.forward, _positionDifference);

        //Returns fals if the enemy is outside of the falshlight's angle (the angle remains unchanged)
        if (_spotLightAngle > 55 * 0.5f) return false;

        //The damage applied to the PC Player depends on the distance between the enemy and flashlight (the closer they are, the greater the damage)
        _damageMultiplier = 1 - (_spotLightDistance / (_notifiedFlashlightRange * 0.5f));
        return true;
    }

    //This checks if the enemy is too close or too far from the PC Player
    (bool _isTooClose, bool _isTooFar) IsCloseToPlayer(Transform _playerTransform)
    {
        float distance = (transform.position - _playerTransform.position).magnitude;

        if (distance <= _tooCloseDistance) return (true, false);
        if(distance > _tooFarDistance)  return (false, true);
        return (false, false);
    }
}
