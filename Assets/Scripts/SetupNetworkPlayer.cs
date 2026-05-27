using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.UI;

using Unity.Netcode;

public class SetupNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private bool isPCPlayer;
    private GameObject pcPlayerMainCamera;
    private InputSystemUIInputModule pcInput;

    
    [SerializeField] private bool isVRPlayer;
    private GameObject vrPlayerMainCamera;
    private XRUIInputModule vrInput;

    void Awake()
    {
        // if (!IsOwner) return;

        // pcPlayerMainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        // Debug.Log("pcPlayerMainCamera: " + pcPlayerMainCamera);

        // pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
        // Debug.Log("pcInput: " + pcInput);

        // foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        // {
        //     if (child.CompareTag("MainCamera"))
        //     {
        //         vrPlayerMainCamera = child.gameObject;
        //         break;
        //     }
        // }
        // Debug.Log("vrPlayerMainCamera: " + vrPlayerMainCamera);

        // vrInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<XRUIInputModule>();
        // Debug.Log("vrInput: " + vrInput);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        pcPlayerMainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Debug.Log("pcPlayerMainCamera: " + pcPlayerMainCamera);

        pcInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<InputSystemUIInputModule>();
        Debug.Log("pcInput: " + pcInput);

        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("MainCamera"))
            {
                vrPlayerMainCamera = child.gameObject;
                break;
            }
        }
        Debug.Log("vrPlayerMainCamera: " + vrPlayerMainCamera);

        vrInput = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<XRUIInputModule>();
        Debug.Log("vrInput: " + vrInput);


        if (isPCPlayer)
        {
            pcPlayerMainCamera.SetActive(true);
            pcInput.enabled = true;

            vrPlayerMainCamera.SetActive(false);
            vrInput.enabled = false;
        }
        else if(isVRPlayer)
        {
            pcPlayerMainCamera.SetActive(false);
            pcInput.enabled = false;

            vrPlayerMainCamera.SetActive(true);
            vrInput.enabled = true;
        }
    }
}
