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
        mapPlayer.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;
        _canPCFunction = false;
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetPCPlayerBodyDataRpc()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
            mapPlayer.gameObject.SetActive(true);

            pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
            pcCamera = pcPlayer.GetComponentInChildren<CinemachineCamera>().gameObject.transform;
            posOffset = mapPlayer.position;
        }
    }

    //Changes the position and rotation of the PC Player map icon to match the PC Player's position and rotation in the level
    void FixedUpdate()
    {
        if(!_canPCFunction) return;

        mapPlayer.position = new Vector3   ((pcPlayer.position.x * multiplier) + posOffset.x, 
                                            (pcPlayer.position.y * multiplier) + posOffset.y, 
                                            (pcPlayer.position.z * multiplier) + posOffset.z);

        mapPlayer.rotation = Quaternion.Euler(0, pcCamera.eulerAngles.y, 0);
    }
}
