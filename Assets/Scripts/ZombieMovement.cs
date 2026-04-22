using UnityEngine;
using UnityEngine.AI;
//This is a single script that allows the zombie nav agent to constantly move towards the player
public class ZombieMovement : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _playerTransform;
    [SerializeField] private float _chaseRange = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, _playerTransform.position) > _chaseRange)
        {
            _agent.SetDestination(_playerTransform.position);
        }
        else
        {
            _agent.SetDestination(transform.position);
        }
    }

    void OnDrawGizmoSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _chaseRange);
    }
}
