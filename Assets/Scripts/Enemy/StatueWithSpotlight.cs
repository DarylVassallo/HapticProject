using UnityEngine;
using UnityEngine.AI;
//This script detects if the statue is within the spotlight, and allows it to move if it is not (modified to use a spotlight instead of the player's camera)(modified to shake the angel when in the light, and damages it).
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class StatueWithSpotlight : MonoBehaviour
{
    private NavMeshAgent _agent;

    private Transform _pcPlayerTransform;
    private Health _pcPlayerHealth;
    private Light _pcPlayerSpotLight;
    private bool _isPCTooClose;
    private bool _isInsidePCSpotLight;

    private Transform _vrPlayerTransform;
    private Health _vrPlayerHealth;
    private Light _vrPlayerSpotLight;
    private bool _isVRTooClose;
    private bool _isInsideVRSpotLight;

    private float _spotLightDistance;
    private float _spotLightAngle;
    
    private Vector3 _destination;
    
    [SerializeField] private float agentSpeed;

    private Vector3 _positionDifference;

    public float speed;
    public float amount;

    private Health health;
    [SerializeField] private float damageToAngel;
    [SerializeField] private float damageToPlayer;
    [SerializeField] private float _tooCloseDistance;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        
        _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        _pcPlayerHealth = _pcPlayerTransform.GetComponent<Health>();
        _pcPlayerSpotLight = _pcPlayerTransform.GetComponentInChildren<Light>();

        _vrPlayerTransform = GameObject.FindGameObjectWithTag("VRPlayer").transform;
        _vrPlayerHealth = _vrPlayerTransform.GetComponent<Health>();
        _vrPlayerSpotLight = _vrPlayerTransform.GetComponentInChildren<Light>();

        health = this.gameObject.GetComponent<Health>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // health.ChangeHealth(-0.2f);
        // transform.position = new Vector3(   transform.position.x + 
        //                                     Mathf.Sin(speed * Time.time * (1 - (health.health / 100))) * amount * (1 - (health.health / 100)),
        //                                     transform.position.y, 
        //                                     transform.position.z);

        _isPCTooClose = IsCloseToPlayer(_pcPlayerTransform);
        _isVRTooClose = IsCloseToPlayer(_vrPlayerTransform);
        if (_isPCTooClose)
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);

            _pcPlayerHealth.ChangeHealth(-damageToPlayer, -1);
        }
        
        if (_isVRTooClose)
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);

            _pcPlayerHealth.ChangeHealth(-damageToPlayer, -1);
        }

        _isInsidePCSpotLight = IsInsideSpotLight(_pcPlayerSpotLight);
        _isInsideVRSpotLight = IsInsideSpotLight(_vrPlayerSpotLight);
        if (_isInsidePCSpotLight && _isInsideVRSpotLight)
        {
            DamageAndFreeze(2);
        }else if (_isInsidePCSpotLight)
        {
            DamageAndFreeze(0);
        }else if (_isInsideVRSpotLight)
        {
            DamageAndFreeze(1);
        }
        else if(!_isPCTooClose && !_isVRTooClose)
        {
            _agent.speed = agentSpeed;
            _destination = _pcPlayerTransform.position;
            _agent.destination = _destination;
        }
    }

    private void DamageAndFreeze(int _playerType)
    {
        _agent.speed = 0;
        _agent.SetDestination(transform.position);

        health.ChangeHealth(-damageToAngel, _playerType);
        transform.position = new Vector3(   
                                        transform.position.x + 
                                        Mathf.Sin(speed * Time.time * (1 - (health.health / 100))) * amount * (1 - (health.health / 100)),
                                        transform.position.y, 
                                        transform.position.z
                                    );
    }

    //Used ChatGPT here
    bool IsInsideSpotLight(Light _playerSpotLight)
    {
        if (!_playerSpotLight.enabled) return false;

        _positionDifference = transform.position - _playerSpotLight.transform.position;
        _spotLightDistance = _positionDifference.magnitude;
        if (_spotLightDistance > (_playerSpotLight.range * 0.5f))
        {
            return false;
        }

        _spotLightAngle = Vector3.Angle(_playerSpotLight.transform.forward, _positionDifference);
        if (_spotLightAngle > _playerSpotLight.spotAngle * 0.5f)
        {
            return false;
        }

        return true;
    }

    bool IsCloseToPlayer(Transform _playerTransform)
    {
        if((transform.position - _playerTransform.position).magnitude <= _tooCloseDistance)  return true;
        return false;
    }
}
