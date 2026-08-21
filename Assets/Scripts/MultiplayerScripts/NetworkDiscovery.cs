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
        StopBroadcasting();
        StopListening();
    }

    //Begins broadcasting message
    public void StartBroadcasting()
    {
        if(isBroadcasting) return;
        isBroadcasting = true;

        serverUdp = new UdpClient();
        serverUdp.EnableBroadcast = true;

        _ = BroadcastLoop();
    }

    //Stops broadcasting message
    public void StopBroadcasting()
    {
        isBroadcasting = false;
        serverUdp?.Close();
        serverUdp = null;
    }

    //Repeated broadcasts a unique message only the client would know
    private async Task BroadcastLoop()
    {
        var endPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
        byte[] data = Encoding.UTF8.GetBytes(broadcastKey);

        while (isBroadcasting)
        {
            serverUdp.Send(data, data.Length, endPoint);
            await Task.Delay(1000);
        }
    }

    //Begins listening for message
    public void StartListening()
    {
        if(isListening) return;
        isListening = true;

        clientUdp = new UdpClient(discoveryPort);
        _ = ListenLoop();
    }

    //Stops listening for message
    public void StopListening()
    {
        isListening = false;
        clientUdp?.Close();
        clientUdp = null;
    }

    //Listens for unique message being broadcast by the host
    private async Task ListenLoop()
    {
        while(isListening)
        {
            try
            {
                var result = await clientUdp.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);

                if(message == broadcastKey)
                {
                    string hostIp = result.RemoteEndPoint.Address.ToString();
                    OnHostFound?.Invoke(hostIp);
                    StopListening();
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e);
                break;
            }
        }
    }
}
