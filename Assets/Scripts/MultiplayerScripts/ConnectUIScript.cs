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
    [SerializeField] private GameObject levelCamera;
    private MultiplayerData multiplayerData;
    private bool isSceneLoaded = false;
    private bool hasSceneLoaded = false;
    private NetworkDiscovery networkDiscovery;



    
    [Header("UI Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;




    [Header("PC Player")]
    [SerializeField] private GameObject pcPlayerStatue;
    [SerializeField] private GameObject pcPlayerWithBody;
    [SerializeField] private GameObject pcPlayerWithoutBody;
    [SerializeField] private GameObject currentPCPlayer;
    [SerializeField] private Transform pcSpawnPoint;
    private ulong pcPlayerClientID;
    




    [Header("VR Player")]
    [SerializeField] private GameObject vrPlayer;
    [SerializeField] private Transform vrSpawnPoint;
    [SerializeField] private GameObject vrRig;
    private bool vrLeftHandActive;
    [SerializeField] private SkinnedMeshRenderer vrLeftHandMesh;
    private bool vrRightHandActive;
    [SerializeField] private SkinnedMeshRenderer vrRightHandMesh;
    private ulong vrPlayerClientID;
    
    

    
    
    [Header("Testing Variables")]
    [SerializeField] private bool isUsingPlayMode;
    [SerializeField] private bool isUsingOnlyVRPlayer;
     private bool _isTestingVRPlayer = false;
    [SerializeField] private bool isUsingOnlyPCPlayer;
    private bool _isTestingPCPlayer = false;

    void Start()
    {
        vrRig.transform.GetChild(0).gameObject.SetActive(false);

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

        EventsManager.OnAddPCPlayerBody += CreatePCPlayerBody;

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

    public override void OnNetworkSpawn()
    {
        // if (!IsOwner) return;

        Debug.Log("ConnectUIScript OnNetworkSpawn");

        levelCamera.SetActive(false);
        vrRig.transform.GetChild(0).gameObject.SetActive(true);
    }

    public override void OnDestroy()
    {
        Debug.Log("ConnectUIScript OnDestroy");
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
    }

    void OnDisable()
    {
        Debug.Log("ConnectUIScript OnDisable");
        if (NetworkManager.Singleton != null)  NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;

        EventsManager.OnAddPCPlayerBody -= CreatePCPlayerBody;
    }

    //If this scene is loaded after the players join the server, 
    // the players will be loaded with the correct bodies, and into the correct positions for the scene
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

    //This obtains the correct spawnpoints for both players
    private void CacheSpawnPoints()
    {
        Debug.Log("ConnectUIScript CacheSpawnPoints");

        vrSpawnPoint = GameObject.Find("VRSpawnpoint")?.transform;
        pcSpawnPoint = GameObject.Find("PCSpawnpoint")?.transform;
    }

    //This creates the players, and assigns them the correct bodies. 
    // This is used only if the scene is loaded, 
    // and the players have already joined the game server
    private void LoadPlayers()
    {
        Debug.Log("ConnectUIScript LoadPlayers");

        if (!NetworkManager.Singleton.IsServer) return;

        //Creates a PC Player if the host player requested to be a PC Player
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
            pcPlayerClientID = 0;
            EventsManager.CreatedPCPlayer();
        }
        else
        {
            //Goes through the two clients, 
            // creates a VR Player for the host player, 
            // and creates a PC Player for the client player
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                bool isHost = clientId == NetworkManager.ServerClientId;

                if(isHost)
                {
                    Debug.Log("Create VRPlayer 1");
                    GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, vrSpawnPoint.rotation);
                    NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                    _vrNetObj.SpawnAsPlayerObject(clientId, true);

                    EventsManager.CreatedVRPlayer();
                }
                else
                {
                    Debug.Log("Create PCPlayer 2");
                    pcPlayerClientID = clientId;
                    EventsManager.CreatedPCPlayer();
                }
            }

            DisableHostButtonsaRpc();
        }
    }

    //Disables the Host login button
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DisableHostButtonsaRpc()
    {
        Debug.Log("ConnectUIScript DisableHostButtonsaRpc");

        Debug.Log("hostButton: " + hostButton);
        Debug.Log("hostButton.transform.parent.gameObject: " + hostButton.transform.parent.gameObject);
        hostButton.transform.parent.gameObject.SetActive(false);
    }

    //Creates a VR/PC Player when someone logs in as host/client
    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log("ConnectUIScript HandleClientConnected 1");
       
        //Creates a PC Player for the host player, playing alone
        if (_isTestingPCPlayer)
        {
            Debug.Log("Create PCPlayer 3");
            pcPlayerClientID = clientId;
            EventsManager.CreatedPCPlayer();

        //Creates a VR Player for the host player, playing alone
        }else if (_isTestingVRPlayer)
        {
            Debug.Log("Create VRPlayer 2");
            GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, vrSpawnPoint.rotation);
            NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
            _vrNetObj.SpawnAsPlayerObject(clientId, true);
            EventsManager.CreatedVRPlayer();
        }else{
            //Creates a VR/PC Player for the current player (a VR Player if this is the host, and a PC Player if this is the client)

            Debug.Log("ConnectUIScript HandleClientConnected 3");

            bool isHost = clientId == NetworkManager.ServerClientId;
            
            if (isHost)
            {
                Debug.Log("ConnectUIScript HandleClientConnected 4");

                Debug.Log("Create VRPlayer 3");
                GameObject _newVRPlayer = Instantiate(vrPlayer, vrSpawnPoint.position, vrSpawnPoint.rotation);
                NetworkObject _vrNetObj = _newVRPlayer.GetComponent<NetworkObject>();
                _vrNetObj.SpawnAsPlayerObject(clientId, true);
                EventsManager.CreatedVRPlayer();
            }
            else
            {
                Debug.Log("ConnectUIScript HandleClientConnected 5");

                Debug.Log("Create PCPlayer 4");
                pcPlayerClientID = clientId;
                EventsManager.CreatedPCPlayer();
            }
        }
    }

    private void CreatePCPlayerBody()
    {
        DestroyPCPlayerStatueRpc();
        GameObject _newPCPlayer = Instantiate(currentPCPlayer, pcSpawnPoint.position, Quaternion.Euler(0f, pcSpawnPoint.eulerAngles.y, 0f));
        NetworkObject _pcNetObj = _newPCPlayer.GetComponent<NetworkObject>();
        _pcNetObj.SpawnAsPlayerObject(pcPlayerClientID, true);

        EventsManager.CreatedPCPlayerBody();
    }

    //Destroys the PC Player statue for both players
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DestroyPCPlayerStatueRpc()
    {
        if(pcPlayerStatue != null) Destroy(pcPlayerStatue);
    }

    //Immediately creates a VR Player for the host (used in debugging only)
    private void DebugStartVRPlayer()
    {
        Debug.Log("ConnectUIScript DebugStartVRPlayer");

        _isTestingVRPlayer = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        NetworkManager.Singleton.StartHost();

        hostButton.transform.parent.gameObject.SetActive(false);
    }

    //Immediately creates a PC Player for the host (used in debugging only)
    private void DebugStartPCPlayer()
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

    //Upon clicking the host button, it creates the host which starts broadcasting a message, 
    // allowing the client player to recieve and connect

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

    //Upon clicking the client button, 
    // it creates the client and listens to a particular message being broadcasted by the host, 
    // which helps connect them together
    private void ClientButtonOnClick()
    {
        Debug.Log("ConnectUIScript ClientButtonOnClick");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if(!isUsingPlayMode)
        {
            networkDiscovery.OnHostFound += OnHostFoundHandler;
            networkDiscovery.StartListening();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        clientButton.transform.parent.gameObject.SetActive(false);
    }

    //Upon finding the host, the client connects to the host's IP, and continues creating the client / PC Player
    private void OnHostFoundHandler(string hostIp)
    {
        Debug.Log("ConnectUIScript OnHostFoundHandler");
        networkDiscovery.OnHostFound -= OnHostFoundHandler;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(hostIp, 7777);

        NetworkManager.Singleton.StartClient();
    }
}
