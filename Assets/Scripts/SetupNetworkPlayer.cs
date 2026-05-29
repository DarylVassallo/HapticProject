using UnityEngine;


using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.UI;

using Unity.Netcode;

public class SetupNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private bool isPCPlayer;
    private GameObject pcPlayerMainCamera;
    private Camera pcPlayerMainCameraCamera;
    private AudioListener pcPlayerMainCameraAudioListener;
    private InputSystemUIInputModule pcInput;

    
    [SerializeField] private bool isVRPlayer;
    private GameObject vrPlayerMainCamera;
    private Camera vrPlayerMainCameraCamera;
    private AudioListener vrPlayerMainCameraAudioListener;
    private XRUIInputModule vrInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        pcPlayerMainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        pcPlayerMainCameraCamera = pcPlayerMainCamera.GetComponent<Camera>();
        pcPlayerMainCameraAudioListener = pcPlayerMainCamera.GetComponent<AudioListener>();
        pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();

        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("MainCamera"))
            {
                vrPlayerMainCamera = child.gameObject;
                break;
            }
        }
        vrPlayerMainCameraCamera = vrPlayerMainCamera.GetComponent<Camera>();
        vrPlayerMainCameraAudioListener = vrPlayerMainCamera.GetComponent<AudioListener>();
        vrInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<XRUIInputModule>();

        if (isPCPlayer)
        {
            SetPCPlayerCamera(true);
            SetVRPlayerCamera(false);

            DisableXRSockets();
        }
        else if(isVRPlayer)
        {
            SetPCPlayerCamera(false);
            SetVRPlayerCamera(true);
        }
    }

    private void SetPCPlayerCamera(bool toggle)
    {
        pcPlayerMainCamera.SetActive(toggle);
        pcPlayerMainCameraCamera.enabled = toggle;
        pcPlayerMainCameraAudioListener.enabled = toggle;
        pcInput.enabled = toggle;
    }

    private void SetVRPlayerCamera(bool toggle)
    {
        vrPlayerMainCamera.SetActive(toggle);
        vrPlayerMainCameraCamera.enabled = toggle;
        vrPlayerMainCameraAudioListener.enabled = toggle;
        vrInput.enabled = toggle;
    }

    public void DisableXRSockets()
    {
        foreach (var socket in FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>())
        {
            socket.enabled = false;
        }
    }
}
