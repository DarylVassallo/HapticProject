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

    void Start()
    {
        hostButton.onClick.AddListener(HostButtonClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
        serverButton.onClick.AddListener(ServerButtonOnClick);
    }

    private  void HostButtonClick()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = vrPlayer;
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = pcPlayer;
    }

    private void ClientButtonOnClick()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = pcPlayer;
        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = vrPlayer;
    }

    private void ServerButtonOnClick()
    {
        NetworkManager.Singleton.StartServer();
    }
}
