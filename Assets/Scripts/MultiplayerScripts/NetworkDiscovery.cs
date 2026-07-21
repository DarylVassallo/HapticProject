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

    public void StartBroadcasting()
    {
        if(isBroadcasting) return;
        isBroadcasting = true;

        serverUdp = new UdpClient();
        serverUdp.EnableBroadcast = true;

        _ = BroadcastLoop();
    }

    public void StopBroadcasting()
    {
        isBroadcasting = false;
        serverUdp?.Close();
        serverUdp = null;
    }

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




    public void StartListening()
    {
        if(isListening) return;
        isListening = true;

        clientUdp = new UdpClient(discoveryPort);
        _ = ListenLoop();
    }

    public void StopListening()
    {
        isListening = false;
        clientUdp?.Close();
        clientUdp = null;
    }

    private async Task ListenLoop()
    {
        while(isListening)
        {
            Debug.Log("NetworkDiscovery ListenLoop loop");
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
            catch (ObjectDisposedException)
            {
                break;
            }
            catch(SocketException se)
            {
                break;
            }
            catch (Exception e)
            {
                break;
            }
        }
    }
}
