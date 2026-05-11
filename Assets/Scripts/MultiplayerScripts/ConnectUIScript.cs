using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

//This script uses UI buttons to create the host, client, and server for the multiplayer network.
public class ConnectUIScript : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;

    [SerializeField] private GameObject pcPlayer;
    [SerializeField] private GameObject vrPlayer;

    private GameObject mainCamera;
    private GameObject vrComponent;

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

        HostButtonClick();
    }

    private  void HostButtonClick()
    {
        Debug.Log("HOST");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainCamera.SetActive(false);
        vrComponent.SetActive(true);

        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = vrPlayer;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = pcPlayer;
    }

    private void ClientButtonOnClick()
    {
        Debug.Log("CLIENT");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        mainCamera.SetActive(true);
        vrComponent.SetActive(false);

        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = pcPlayer;
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = vrPlayer;
    }

    private void ServerButtonOnClick()
    {
        NetworkManager.Singleton.StartServer();
    }
}
