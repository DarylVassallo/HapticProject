using UnityEngine;


using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.UI;

using Unity.Netcode;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        // if (!IsOwner) return;

        GameObject.FindGameObjectWithTag("VRRig").transform.GetChild(0).gameObject.SetActive(true);

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
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MainCamera"))
        {
            if (obj.layer == LayerMask.NameToLayer("PCCamera"))
            {
                pcPlayerMainCamera = obj;
                break;
            }
        }

        pcPlayerMainCameraCamera = pcPlayerMainCamera.GetComponent<Camera>();
        pcPlayerMainCameraAudioListener = pcPlayerMainCamera.GetComponent<AudioListener>();
        pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
    }

    private void VRPlayerReferences()
    {
        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
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

    //This toggles the PC Camera, and input (disables if the VR Player is the owner, and enables if it is the PC Player)
    private void SetPCPlayerCamera(bool toggle)
    {
        pcPlayerMainCameraCamera.enabled = toggle;
        pcPlayerMainCameraAudioListener.enabled = toggle;
        pcInput.enabled = toggle;
    }

    //This toggles the VR Camera, and input (disables if the PC Player is the owner, and enables if it is the VR Player)
    private void SetVRPlayerCamera(bool toggle)
    {
        vrPlayerMainCameraCamera.enabled = toggle;
        vrPlayerMainCameraAudioListener.enabled = toggle;
        vrInput.enabled = toggle;
    }

    //This disables all XR Sockets in the scene
    private void DisableXRSockets()
    {
        foreach (var socket in FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>())
        {
            socket.enabled = false;
        }
    }
}