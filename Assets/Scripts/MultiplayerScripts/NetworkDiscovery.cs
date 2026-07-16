using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//This was formed using Claude
public class NetworkDiscovery : MonoBehaviour
{
    [SerializeField] private int discoveryPort = 47777;
    [SerializeField] private string broadcastKey = "MY_GAME_DISCOVERY";

    private bool isBroadcasting;
    private bool isListening;

    private UdpClient serverUdp;
    private UdpClient clientUdp;

    public event Action<string> OnHostFound;

    private void OnDestroy()
    {
        Debug.Log("NetworkDiscovery OnDestroy");
        StopBroadcasting();
        StopListening();
    }

    public void StartBroadcasting()
    {
        Debug.Log("NetworkDiscovery StartBroadcasting");
        if(isBroadcasting) return;
        isBroadcasting = true;

        serverUdp = new UdpClient();
        serverUdp.EnableBroadcast = true;

        _ = BroadcastLoop();
    }

    public void StopBroadcasting()
    {
        Debug.Log("NetworkDiscovery StopBroadcasting");
        isBroadcasting = false;
        serverUdp?.Close();
        serverUdp = null;
    }

    private async Task BroadcastLoop()
    {
        Debug.Log("NetworkDiscovery BroadcastLoop");
        var endPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
        byte[] data = Encoding.UTF8.GetBytes(broadcastKey);

        while (isBroadcasting)
        {
            Debug.Log("NetworkDiscovery BroadcastLoop data : " + data);
            Debug.Log("NetworkDiscovery BroadcastLoop data.Length : " + data.Length);
            Debug.Log("NetworkDiscovery BroadcastLoop endPoint : " + endPoint);
            serverUdp.Send(data, data.Length, endPoint);
            await Task.Delay(1000);
        }
    }




    public void StartListening()
    {
        Debug.Log("NetworkDiscovery StartListening");
        if(isListening) return;
        isListening = true;

        clientUdp = new UdpClient(discoveryPort);
        _ = ListenLoop();
    }

    public void StopListening()
    {
        Debug.Log("NetworkDiscovery StopListening");
        isListening = false;
        clientUdp?.Close();
        clientUdp = null;
    }

    private async Task ListenLoop()
    {
        Debug.Log("NetworkDiscovery ListenLoop");
        Debug.Log("NetworkDiscovery ListenLoop isListening : " + isListening);
        while(isListening)
        {
            Debug.Log("NetworkDiscovery ListenLoop loop");
            try
            {
                var result = await clientUdp.ReceiveAsync();
                Debug.Log("NetworkDiscovery ListenLoop result : " + result);
                string message = Encoding.UTF8.GetString(result.Buffer);
                Debug.Log("NetworkDiscovery ListenLoop message : " + message);
                Debug.Log("NetworkDiscovery ListenLoop broadcastKey : " + broadcastKey);

                if(message == broadcastKey)
                {
                    string hostIp = result.RemoteEndPoint.Address.ToString();
                    Debug.Log("Found Host IP: " + hostIp);
                    OnHostFound?.Invoke(hostIp);
                    StopListening();
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.LogError("NetworkDiscovery ObjectDisposedException");
                break;
            }
            catch(SocketException se)
            {
                Debug.LogError("NetworkDiscovery SocketException: " + se.SocketErrorCode + " " + se.Message);
                break;
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkDiscovery ListenLoop exception: " + e);
                break;
            }
        }
        Debug.Log("NetworkDiscovery ListenLoop exited");
    }
}
