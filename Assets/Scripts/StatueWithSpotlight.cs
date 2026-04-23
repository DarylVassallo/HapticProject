using UnityEngine;
using UnityEngine.AI;
//This script detects if the statue is within the spotlight, and allows it to move if it is not (modified to use a spotlight instead of the player's camera).
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class StatueWithSpotlight : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _playerTransform;
    private Vector3 _destination;
    private Light _spotLight;
    [SerializeField] private float agentSpeed;

    private float _spotLightDistance;
    private float _spotLightAngle;
    private Vector3 _positionDifference;

    public float speed;
    public float amount;

    private Health health;
    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _spotLight = _playerTransform.GetComponentInChildren<Light>();

        health = this.gameObject.GetComponent<Health>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsInsideSpotLight())
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);

            health.ChangeHealth(-0.1f);
            transform.position = new Vector3(   transform.position.x + 
                                                    Mathf.Sin   (   Time.time * speed * (health.health / 100)) * 
                                                                    amount * (health.health / 100), 
                                                transform.position.y, 
                                                transform.position.z);
        }
        else
        {
            _agent.speed = agentSpeed;
            _destination = _playerTransform.position;
            _agent.destination = _destination;
        }
    }

    //Used ChatGPT here
    bool IsInsideSpotLight()
    {
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
}
