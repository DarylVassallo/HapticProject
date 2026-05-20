using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.UI;

using Unity.Netcode.Transports.UTP;

//This script uses UI buttons to create the host, client, and server for the multiplayer network.
public class ConnectUIScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;

    [SerializeField] private GameObject pcPlayer;
    [SerializeField] private Transform pcSpawnPoint;

    [SerializeField] private GameObject vrPlayer;
    [SerializeField] private Transform vrSpawnPoint;

    private GameObject mainCamera;
    private InputSystemUIInputModule pcInput;

    private GameObject vrComponent;
    private XRUIInputModule vrInput;


    public static event Action OnCreatedPCPlayer;
    public static event Action OnCreatedVRPlayer;

    private bool _isTestingVRPlayer = false;
    private bool _isTestingPCPlayer = false;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();

        vrComponent = GameObject.FindGameObjectWithTag("VRPlayer");
        vrInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<XRUIInputModule>();

        Debug.Log("mainCamera: " + mainCamera);
        Debug.Log("vrComponent: " + vrComponent);

        hostButton.onClick.AddListener(HostButtonClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
        serverButton.onClick.AddListener(ServerButtonOnClick);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // DebugStartVRPlayer();
        // DebugStartPCPlayer();
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (_isTestingPCPlayer)
        {
            GameObject _newPCPlayer = Instantiate(pcPlayer, pcSpawnPoint.position, Quaternion.identity);
            NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();
            _pcNetObj.SpawnAsPlayerObject(clientId, true);
            OnCreatedPCPlayer?.Invoke();
        }else if (_isTestingVRPlayer)
        {
            GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
            NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
            _vrNetObj.SpawnAsPlayerObject(clientId, true);
            OnCreatedVRPlayer?.Invoke();
        }else{
            if (clientId == 0)
            {
                GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
                NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                _vrNetObj.SpawnAsPlayerObject(clientId, true);
                OnCreatedVRPlayer?.Invoke();
            }else if (clientId == 1)
            {
                GameObject _newPCPlayer = Instantiate(pcPlayer, pcSpawnPoint.position, Quaternion.identity);
                NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();
                _pcNetObj.SpawnAsPlayerObject(clientId, true);
                OnCreatedPCPlayer?.Invoke();
            }
        }
        
    }

    private  void DebugStartVRPlayer()
    {
        _isTestingVRPlayer = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(false);
        pcInput.enabled = false;

        vrComponent.SetActive(true);
        vrInput.enabled = true;

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void DebugStartPCPlayer()
    {
        _isTestingPCPlayer = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(true);
        pcInput.enabled = true;

        vrComponent.SetActive(false);
        vrInput.enabled = false;

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void HostButtonClick()
    {
        Debug.Log("HOST");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(false);
        pcInput.enabled = false;

        vrComponent.SetActive(true);
        vrInput.enabled = true;

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private void ClientButtonOnClick()
    {
        Debug.Log("CLIENT");

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "192.168.1.10";
        transport.ConnectionData.Port = 7777;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        mainCamera.SetActive(true);
        pcInput.enabled = true;

        vrComponent.SetActive(false);
        vrInput.enabled = false;

        NetworkManager.Singleton.StartClient();

        clientButton.transform.parent.gameObject.SetActive(false);
    }

    private void ServerButtonOnClick()
    {
        NetworkManager.Singleton.StartServer();
    }
}
