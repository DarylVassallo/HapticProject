using UnityEngine;

using Unity.Netcode;
using Unity.Cinemachine;

public class MapPlayerTracker : NetworkBehaviour
{
    private bool _setupPlayerIcon;

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
        EventsManager.OnReachedSwitches += ReachedSwitchesRpc;
        EventsManager.OnStartWithPlayerIcon += StartWithPlayerIcon;
    }

    private void OnDisable()
    {
        EventsManager.OnReachedSwitches -= ReachedSwitchesRpc;
        EventsManager.OnStartWithPlayerIcon -= StartWithPlayerIcon;
        _canPCFunction = false;
    }

    private void StartWithPlayerIcon()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            ReachedSwitchesRpc();
        }
        else
        {
            EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyData;
        }
    }

    private void GetPCPlayerBodyData()
    {
        ReachedSwitchesRpc();
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyData;
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReachedSwitchesRpc()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null && !_setupPlayerIcon)
        {
            _canPCFunction = true;
            _setupPlayerIcon = true;

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
                                            mapPlayer.position.y, 
                                            (pcPlayer.position.z * multiplier) + posOffset.z);

        mapPlayer.rotation = Quaternion.Euler(0, pcCamera.eulerAngles.y, 0);
    }
}
