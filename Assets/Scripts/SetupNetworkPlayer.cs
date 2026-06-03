using UnityEngine;


using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.UI;

using Unity.Netcode;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SetupNetworkPlayer : NetworkBehaviour
{
    public bool isPCPlayer;
    private GameObject pcPlayerMainCamera;
    private Camera pcPlayerMainCameraCamera;
    private AudioListener pcPlayerMainCameraAudioListener;
    private InputSystemUIInputModule pcInput;
    
    public bool isVRPlayer;
    private GameObject vrPlayerMainCamera;
    private Camera vrPlayerMainCameraCamera;
    private AudioListener vrPlayerMainCameraAudioListener;
    private XRUIInputModule vrInput;

    public override void OnNetworkSpawn()
    {
        // if (!IsOwner) return;

        PCPlayerReferences();
        VRPlayerReferences();

        if (isPCPlayer && IsOwner)
        {
            PCPlayerSetup();
        }
        else if(isVRPlayer && IsOwner)
        {
            VRPlayerSetup();
        }
    }

    private void PCPlayerReferences()
    {
        pcPlayerMainCamera = GameObject.FindGameObjectWithTag("PCCamera");
        pcPlayerMainCameraCamera = pcPlayerMainCamera.GetComponent<Camera>();
        pcPlayerMainCameraAudioListener = pcPlayerMainCamera.GetComponent<AudioListener>();
        pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
    }

    private void VRPlayerReferences()
    {
        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("VRCamera"))
            {
                vrPlayerMainCamera = child.gameObject;
                break;
            }
        }

        vrPlayerMainCameraCamera = vrPlayerMainCamera.GetComponent<Camera>();
        vrPlayerMainCameraAudioListener = vrPlayerMainCamera.GetComponent<AudioListener>();
        vrInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<XRUIInputModule>();
    }

    private void PCPlayerSetup()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPCPlayerCamera(true);
        SetVRPlayerCamera(false);

        DisableXRSockets();
    }

    private void VRPlayerSetup()
    {
        SetPCPlayerCamera(false);
        SetVRPlayerCamera(true);
    }

    private void SetPCPlayerCamera(bool toggle)
    {
        pcPlayerMainCameraCamera.enabled = toggle;
        pcPlayerMainCameraAudioListener.enabled = toggle;
        pcInput.enabled = toggle;
    }

    private void SetVRPlayerCamera(bool toggle)
    {
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