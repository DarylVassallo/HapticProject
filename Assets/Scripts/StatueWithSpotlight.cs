using UnityEngine;
using UnityEngine.AI;
//This script detects if the statue is within the spotlight, and allows it to move if it is not (modified to use a spotlight instead of the player's camera)(modified to shake the angel when in the light, and damages it).
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class StatueWithSpotlight : MonoBehaviour
{
    private NavMeshAgent _agent;

    private Transform _playerTransform;
    private Health _playerHealth;
    
    private Vector3 _destination;
    private Light _spotLight;
    [SerializeField] private float agentSpeed;

    private float _spotLightDistance;
    private float _spotLightAngle;
    private Vector3 _positionDifference;

    public float speed;
    public float amount;

    private Health health;
    [SerializeField] private float damageToAngel;
    [SerializeField] private float damageToPlayer;
    [SerializeField] private float _tooCloseDistance;

    private bool _isTooClose;
    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerHealth = _playerTransform.GetComponent<Health>();

        _spotLight = _playerTransform.GetComponentInChildren<Light>();

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

        _isTooClose = IsCloseToPlayer();
        if (_isTooClose)
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);

            _playerHealth.ChangeHealth(-damageToPlayer);
        }

        if (IsInsideSpotLight())
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);

            health.ChangeHealth(-damageToAngel);
            transform.position = new Vector3(   
                                            transform.position.x + 
                                            Mathf.Sin(speed * Time.time * (1 - (health.health / 100))) * amount * (1 - (health.health / 100)),
                                            transform.position.y, 
                                            transform.position.z
                                        );
        }
        else if(!_isTooClose)
        {
            _agent.speed = agentSpeed;
            _destination = _playerTransform.position;
            _agent.destination = _destination;
        }
    }

    //Used ChatGPT here
    bool IsInsideSpotLight()
    {
        if (!_spotLight.enabled) return false;

        _positionDifference = (transform.position - _spotLight.transform.position);
        _spotLightDistance = _positionDifference.magnitude;
        if (_spotLightDistance > (_spotLight.range * 0.5f))
        {
            return false;
        }

        _spotLightAngle = Vector3.Angle(_spotLight.transform.forward, _positionDifference);
        if (_spotLightAngle > _spotLight.spotAngle * 0.5f)
        {
            return false;
        }

        return true;
    }

    bool IsCloseToPlayer()
    {
        if((transform.position - _playerTransform.position).magnitude <= _tooCloseDistance)  return true;
        return false;
    }
}
