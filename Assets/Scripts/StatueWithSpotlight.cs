using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float _agentSpeed;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _spotLight = _playerTransform.GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInsideSpotLight())
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);
        }
        else
        {
            _agent.speed = _agentSpeed;
            _destination = _playerTransform.position;
            _agent.destination = _destination;
        }
    }

    //Used ChatGPT here
    bool IsInsideSpotLight()
    {
        Vector3 lightPos = _spotLight.transform.position;
        Vector3 lightDir = _spotLight.transform.forward;

        Vector3 toObject = transform.position - lightPos;
        float distance = toObject.magnitude;

        if (distance > (_spotLight.range * 0.5f))
        {
            return false;
        }

        float angle = Vector3.Angle(lightDir, toObject);
        if (angle > _spotLight.spotAngle * 0.5f)
        {
            return false;
        }

        return true;
    }
}
