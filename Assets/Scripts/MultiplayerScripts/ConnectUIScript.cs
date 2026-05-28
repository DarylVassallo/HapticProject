using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

using Unity.Netcode.Transports.UTP;

using UnityEngine.SceneManagement;
using System.Collections.Generic;

//This script uses UI buttons to create the host, client, and server for the multiplayer network.
public class ConnectUIScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;

    [SerializeField] private GameObject pcPlayerWithBody;
    [SerializeField] private GameObject pcPlayerWithoutBody;
    [SerializeField] private GameObject currentPCPlayer;
    [SerializeField] private Transform pcSpawnPoint;

    [SerializeField] private GameObject vrPlayer;
    [SerializeField] private Transform vrSpawnPoint;

    public static event Action OnCreatedPCPlayer;
    public static event Action OnCreatedVRPlayer;

    private bool _isTestingVRPlayer = false;
    private bool _isTestingPCPlayer = false;

    [SerializeField] private bool isUsingPlayMode;

    void Start()
    {
        Debug.Log("Start");

        hostButton.onClick.AddListener(HostButtonClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
        serverButton.onClick.AddListener(ServerButtonOnClick);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("OnEnable");
        Debug.Log("NetworkManager.Singleton: " + NetworkManager.Singleton);
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        DebugStartVRPlayer();
        // DebugStartPCPlayer();
    }

    private void OnDestroy()
    {
        Debug.Log("OnDisable");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    private void RegisterSceneEvents()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("No NetworkManager yet");
            return;
        }

        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogWarning("SceneManager is NULL");
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
    }

    private void SceneLoaded(   string sceneName, 
                                LoadSceneMode mode, 
                                List<ulong> clientsCompleted, 
                                List<ulong> clientsTimedOut)
    {
        Debug.Log("Scene Loaded!!");
        Debug.Log("sceneName: " + sceneName);
        Debug.Log("mode: " + mode);
        Debug.Log("clientsCompleted: " + clientsCompleted);
        Debug.Log("clientsTimedOut: " + clientsTimedOut);

        if(sceneName == "SauronLevelScene")
        {
            currentPCPlayer = pcPlayerWithBody;
        }else if(sceneName == "GameOverScene")
        {
            currentPCPlayer = pcPlayerWithoutBody;
        }

        CacheSpawnPoints();
        LoadPlayers();
    }

    private void CacheSpawnPoints()
    {
        vrSpawnPoint = GameObject.Find("VRSpawnpoint")?.transform;
        pcSpawnPoint = GameObject.Find("PCSpawnpoint")?.transform;

        Debug.Log("new vrSpawnPoint: " + vrSpawnPoint);
        Debug.Log("new pcSpawnPoint: " + pcSpawnPoint);
    }

    private void LoadPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("Spawn client: " + clientId);

            if(clientId == 0)
            {
                Debug.Log("Spawn VR Player");
                Debug.Log("vrPlayer: " + vrPlayer);
                Debug.Log("vrSpawnPoint.position: " + vrSpawnPoint.position);

                GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
                NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                _vrNetObj.SpawnAsPlayerObject(0, true);
                Debug.Log("Create VR");
                OnCreatedVRPlayer?.Invoke();
            }else if(clientId == 1)
            {
                Debug.Log("Spawn PC Player");
                Debug.Log("currentPCPlayer: " + currentPCPlayer);
                Debug.Log("pcSpawnPoint.position: " + pcSpawnPoint.position);

                GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
                NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();
                _pcNetObj.SpawnAsPlayerObject(1, true);
                Debug.Log("Create PC");
                OnCreatedPCPlayer?.Invoke();
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("HandleClientConnected");
        if (!NetworkManager.Singleton.IsServer) return;

        if (_isTestingPCPlayer)
        {
            GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
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
            Debug.Log("clientId: " + clientId);
            if (clientId == 0)
            {
                Debug.Log("Spawn VR Player");
                GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
                NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                _vrNetObj.SpawnAsPlayerObject(clientId, true);
                Debug.Log("Create VR");
                OnCreatedVRPlayer?.Invoke();
            }else if (clientId == 1)
            {
                Debug.Log("Spawn PC Player");
                GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
                NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();
                _pcNetObj.SpawnAsPlayerObject(clientId, true);
                Debug.Log("Create PC");
                OnCreatedPCPlayer?.Invoke();
            }
        }
        
        RegisterSceneEvents();
    }

    private  void DebugStartVRPlayer()
    {
        Debug.Log("DebugStartVRPlayer");
        _isTestingVRPlayer = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void DebugStartPCPlayer()
    {
        Debug.Log("DebugStartPCPlayer");
        _isTestingPCPlayer = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private  void HostButtonClick()
    {
        Debug.Log("HostButtonClick");
        Debug.Log("HOST");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(!isUsingPlayMode)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", 7777);
        }
        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private void ClientButtonOnClick()
    {
        Debug.Log("ClientButtonOnClick");
        Debug.Log("CLIENT");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if(!isUsingPlayMode)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("192.168.1.10", 7777);
        }
        NetworkManager.Singleton.StartClient();

        clientButton.transform.parent.gameObject.SetActive(false);
    }

    private void ServerButtonOnClick()
    {
        Debug.Log("ServerButtonOnClick");
        NetworkManager.Singleton.StartServer();
    }
}
