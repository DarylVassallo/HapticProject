using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

using Unity.Netcode.Transports.UTP;

using UnityEngine.SceneManagement;
using System.Collections.Generic;

using System.Collections;

//This script uses UI buttons to create the host, client, and server for the multiplayer network.
public class ConnectUIScript : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [SerializeField] private GameObject pcPlayerWithBody;
    [SerializeField] private GameObject pcPlayerWithoutBody;
    [SerializeField] private GameObject currentPCPlayer;
    [SerializeField] private Transform pcSpawnPoint;

    [SerializeField] private GameObject vrPlayer;
    [SerializeField] private Transform vrSpawnPoint;

    [SerializeField] private GameObject vrLeftHand;
    [SerializeField] private SkinnedMeshRenderer vrLeftHandMesh;
    [SerializeField] private GameObject vrRightHand;
    [SerializeField] private SkinnedMeshRenderer vrRightHandMesh;

    public static event Action OnCreatedPCPlayer;
    public static event Action OnCreatedVRPlayer;

    private bool _isTestingVRPlayer = false;
    private bool _isTestingPCPlayer = false;

    [SerializeField] private bool isUsingPlayMode;
    [SerializeField] private bool isUsingOnlyPCPlayer;
    [SerializeField] private bool isUsingOnlyVRPlayer;

    private MultiplayerData multiplayerData;
    private bool isSceneLoaded = false;

    private bool hasSceneLoaded = false;

    private NetworkDiscovery networkDiscovery;

    void Start()
    {
        networkDiscovery = this.GetComponent<NetworkDiscovery>();
        Debug.Log("ConnectUIScript Start");

        Debug.Log("ConnectUIScript Start isSceneLoaded: " + isSceneLoaded);
        if (isSceneLoaded)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            hostButton.onClick.AddListener(HostButtonClick);
            clientButton.onClick.AddListener(ClientButtonOnClick);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("ConnectUIScript Start hasSceneLoaded: " + hasSceneLoaded);
            if(!hasSceneLoaded)
            {
                hasSceneLoaded = true;

                Debug.Log("ConnectUIScript Start NetworkManager.Singleton.SceneManager: " + NetworkManager.Singleton.SceneManager);
                Debug.Log("ConnectUIScript Start NetworkManager.Singleton.IsServer: " + NetworkManager.Singleton.IsServer);

                if(NetworkManager.Singleton.SceneManager != null && NetworkManager.Singleton.IsServer)
                {
                    Debug.Log("ConnectUIScript Start Add SceneLoaded");
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
                } 
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            }

            if(isUsingOnlyVRPlayer) DebugStartVRPlayer();
            if(isUsingOnlyPCPlayer) DebugStartPCPlayer();
        }
    }

    void OnEnable()
    {
        Debug.Log("ConnectUIScript OnEnable");
        
        Debug.Log("ConnectUIScript OnEnable NetworkManager.Singleton: " + NetworkManager.Singleton);
        if(NetworkManager.Singleton != null)
        {
            Debug.Log("ConnectUIScript OnEnable hasSceneLoaded: " + hasSceneLoaded);
            if(!hasSceneLoaded)
            {
                hasSceneLoaded = true;

                Debug.Log("ConnectUIScript OnEnable NetworkManager.Singleton.SceneManager: " + NetworkManager.Singleton.SceneManager);
                Debug.Log("ConnectUIScript OnEnable NetworkManager.Singleton.IsServer: " + NetworkManager.Singleton.IsServer);

                if(NetworkManager.Singleton.SceneManager != null && NetworkManager.Singleton.IsServer)
                {
                    Debug.Log("ConnectUIScript OnEnable Add SceneLoaded");
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
                }
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            }
        }
    }

    IEnumerator SetupNetwork(float delay)
    {
        Debug.Log("ConnectUIScript SetupNetwork");

        yield return new WaitForSeconds(delay);

        if(NetworkManager.Singleton.SceneManager != null) NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDestroy()
    {
        Debug.Log("ConnectUIScript OnDestroy");
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    void OnDisable()
    {
        Debug.Log("ConnectUIScript OnDisable");
        if (NetworkManager.Singleton != null)  NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
    }

    private void SceneLoaded(   string sceneName, 
                                LoadSceneMode mode, 
                                List<ulong> clientsCompleted, 
                                List<ulong> clientsTimedOut)
    {
        Debug.Log("ConnectUIScript SceneLoaded");

        isSceneLoaded = true;

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
        Debug.Log("ConnectUIScript CacheSpawnPoints");

        vrSpawnPoint = GameObject.Find("VRSpawnpoint")?.transform;
        pcSpawnPoint = GameObject.Find("PCSpawnpoint")?.transform;
    }

    private void LoadPlayers()
    {
        Debug.Log("ConnectUIScript LoadPlayers");

        if (!NetworkManager.Singleton.IsServer) return;

        multiplayerData = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<MultiplayerData>();
        if (multiplayerData.isFirstPlayerPCPlayer)
        {
            _isTestingPCPlayer = true;
            if(currentPCPlayer == pcPlayerWithBody)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            hostButton.transform.parent.gameObject.SetActive(false);
            
            Debug.Log("Create PCPlayer 1");
            GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
            _newPCPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(0, true);

            ActivateVRHandsRpc();

            OnCreatedPCPlayer?.Invoke();
        }
        else
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                bool isHost = clientId == NetworkManager.ServerClientId;

                if(isHost)
                {
                    Debug.Log("Create VRPlayer 1");
                    GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
                    NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                    _vrNetObj.SpawnAsPlayerObject(clientId, true);

                    OnCreatedVRPlayer?.Invoke();
                }
                else
                {
                    Debug.Log("Create PCPlayer 2");
                    Debug.Log("pcSpawnPoint: " + pcSpawnPoint);
                    GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
                    NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();

                    ActivateVRHandsRpc();
                    
                    _pcNetObj.SpawnAsPlayerObject(clientId, true);

                    OnCreatedPCPlayer?.Invoke();
                }
            }

            // StartCoroutine(DisableHostButtonsDelay(1f));
            DisableHostButtonsaRpc();
        }
    }

    IEnumerator DisableHostButtonsDelay(float delay)
    {
        Debug.Log("ConnectUIScript DisableHostButtonsDelay");

        yield return new WaitForSeconds(delay);

        DisableHostButtonsaRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DisableHostButtonsaRpc()
    {
        Debug.Log("ConnectUIScript DisableHostButtonsaRpc");

        Debug.Log("hostButton: " + hostButton);
        Debug.Log("hostButton.transform.parent.gameObject: " + hostButton.transform.parent.gameObject);
        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("ConnectUIScript HandleClientConnected 1");

        if (!NetworkManager.Singleton.IsServer) return;
       
        Debug.Log("ConnectUIScript HandleClientConnected 2");

        if (_isTestingPCPlayer)
        {
            Debug.Log("Create PCPlayer 3");
            GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
            NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();

            ActivateVRHandsRpc();

            _pcNetObj.SpawnAsPlayerObject(clientId, true);
            OnCreatedPCPlayer?.Invoke();
        }else if (_isTestingVRPlayer)
        {
            Debug.Log("Create VRPlayer 2");
            GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
            NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
            _vrNetObj.SpawnAsPlayerObject(clientId, true);
            OnCreatedVRPlayer?.Invoke();
        }else{
            Debug.Log("ConnectUIScript HandleClientConnected 3");

            bool isHost = clientId == NetworkManager.ServerClientId;
            
            if (isHost)
            {
                Debug.Log("ConnectUIScript HandleClientConnected 4");

                Debug.Log("Create VRPlayer 3");
                GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, Quaternion.identity);
                NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                _vrNetObj.SpawnAsPlayerObject(clientId, true);
                OnCreatedVRPlayer?.Invoke();
            }
            else
            {
                Debug.Log("ConnectUIScript HandleClientConnected 5");

                Debug.Log("Create PCPlayer 4");
                Debug.Log("pcSpawnPoint: " + pcSpawnPoint);
                GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.identity);
                NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();

                ActivateVRHandsRpc();
                
                _pcNetObj.SpawnAsPlayerObject(clientId, true);
                OnCreatedPCPlayer?.Invoke();
            }
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateVRHandsRpc()
    {
        vrLeftHand.SetActive(true);
        vrLeftHandMesh.enabled = true;

        vrRightHand.SetActive(true);
        vrRightHandMesh.enabled = true;
    }

    public void DebugStartVRPlayer()
    {
        Debug.Log("ConnectUIScript DebugStartVRPlayer");

        _isTestingVRPlayer = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    public void DebugStartPCPlayer()
    {
        Debug.Log("ConnectUIScript DebugStartPCPlayer");

        _isTestingPCPlayer = true;


        if(currentPCPlayer == pcPlayerWithBody)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);

        multiplayerData = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<MultiplayerData>();
        multiplayerData.isFirstPlayerPCPlayer = true;
    }

    //NetworkManager aspect created using Claude
    private  void HostButtonClick()
    {
        Debug.Log("ConnectUIScript HostButtonClick");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(!isUsingPlayMode)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", 7777);
            networkDiscovery.StartBroadcasting();
        }
        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    private void ClientButtonOnClick()
    {
        Debug.Log("ConnectUIScript ClientButtonOnClick");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if(!isUsingPlayMode)
        {
            networkDiscovery.OnHostFound += OnHostFoundHandler;
            networkDiscovery.StartListening();

            // var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            // transport.SetConnectionData("192.168.1.10", 7777);
            // NetworkManager.Singleton.StartClient();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        clientButton.transform.parent.gameObject.SetActive(false);
    }

    private void OnHostFoundHandler(string hostIp)
    {
        Debug.Log("ConnectUIScript OnHostFoundHandler");
        networkDiscovery.OnHostFound -= OnHostFoundHandler;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(hostIp, 7777);

        NetworkManager.Singleton.StartClient();
    }
}
