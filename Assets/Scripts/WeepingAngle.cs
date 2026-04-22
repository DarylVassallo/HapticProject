using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//This script detects if the statue is within the player's view, and allows it to move if it is not.
//Source: https://www.youtube.com/watch?v=_e57zSZSOS8
public class WeepingAngle : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _playerTransform;
    private Vector3 _destination;
    private Camera _playerCamera;
    [SerializeField] private float _agentSpeed;

    private Plane[] _planes;

    void Awake()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerCamera = _playerTransform.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        _planes = GeometryUtility.CalculateFrustumPlanes(_playerCamera);

        if(GeometryUtility.TestPlanesAABB(_planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            _agent.speed = 0;
            _agent.SetDestination(transform.position);
        }
        if(!GeometryUtility.TestPlanesAABB(_planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            _agent.speed = _agentSpeed;
            _destination = _playerTransform.position;
            _agent.destination = _destination;
        }
    }
}
