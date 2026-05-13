using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

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
    private GameObject vrComponent;

    public static event Action OnCreatedPCPlayer;
    public static event Action OnCreatedVRPlayer;

    private bool _isTestingVRPlayer = false;
    private bool _isTestingPCPlayer = false;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        vrComponent = GameObject.FindGameObjectWithTag("VRPlayer");

        Debug.Log("mainCamera: " + mainCamera);
        Debug.Log("vrComponent: " + vrComponent);

        hostButton.onClick.AddListener(HostButtonClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
        serverButton.onClick.AddListener(ServerButtonOnClick);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // DebugStartVRPlayer();
        DebugStartPCPlayer();
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
        vrComponent.SetActive(true);

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void DebugStartPCPlayer()
    {
        _isTestingPCPlayer = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(true);
        vrComponent.SetActive(false);

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void HostButtonClick()
    {
        Debug.Log("HOST");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(false);
        vrComponent.SetActive(true);

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private void ClientButtonOnClick()
    {
        Debug.Log("CLIENT");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        mainCamera.SetActive(true);
        vrComponent.SetActive(false);

        NetworkManager.Singleton.StartClient();

        clientButton.transform.parent.gameObject.SetActive(false);
    }

    private void ServerButtonOnClick()
    {
        NetworkManager.Singleton.StartServer();
    }
}
