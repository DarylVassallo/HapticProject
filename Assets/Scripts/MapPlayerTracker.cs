using UnityEngine;

using Unity.Netcode;
using Unity.Cinemachine;

public class MapPlayerTracker : NetworkBehaviour
{
    [SerializeField] private Transform mapPlayer;
    private Transform pcPlayer;
    private Transform pcCamera;

    [SerializeField] private float multiplier;

    private bool _canPCFunction = false;

    private Vector3 posOffset;

    void Awake()
    {
        _canPCFunction = false;
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerDataRpc;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerDataRpc;
    }
    
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void GetPCPlayerDataRpc()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
            pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
            Debug.Log("pcPlayer: " + pcPlayer);
            Debug.Log("pcPlayer.GetComponentInChildren<CinemachineCamera>(): " + pcPlayer.GetComponentInChildren<CinemachineCamera>());
            
            pcCamera = pcPlayer.GetComponentInChildren<CinemachineCamera>().gameObject.transform;
            Debug.Log("pcCamera: " + pcCamera);

            posOffset = mapPlayer.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!_canPCFunction) return;

        mapPlayer.position = new Vector3   ((pcPlayer.position.x * multiplier) + posOffset.x, 
                                            (pcPlayer.position.y * multiplier) + posOffset.y, 
                                            (pcPlayer.position.z * multiplier) + posOffset.z);

        mapPlayer.rotation = Quaternion.Euler(0, pcCamera.eulerAngles.y, 0);
    }
}
