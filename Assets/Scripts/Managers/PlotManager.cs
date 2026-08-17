using UnityEngine;
using System.Collections;

using Unity.Netcode;
public class PlotManager : NetworkBehaviour
{
    private bool _constantlyCheck;
    private bool _checkRoof;

    [Header("VR Head")]
    private Transform vrPlayerCamera;
    private Material vrPlayerCameraCover;

    [Header("PC Statue Pipes")]
    [SerializeField] private GameObject pcStatue;
    [SerializeField] private GameObject[] pcPipes;
    private int visiblePCPipes = 0;
    [SerializeField] private float pcSpawnDelay;

    [Header("PC Tutorial UI")]
    [SerializeField] private GameObject pcMovementTutorial;
    [SerializeField] private GameObject pcChargeTutorial;
    private bool _hasSeenChargeTutorial;
    [SerializeField] private GameObject pcInteractTutorial;
    private bool _hasSeenInteractTutorial;

    [Header("Roof")]
    [SerializeField] private Transform roof;
    

    private void Awake()
    {
        ToggleTutorial(null);

        for(int i = 0; i < pcPipes.Length; i++)
        {
            pcPipes[i].SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayer += GetPCPlayerData;
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyData;
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData;

        EventsManager.OnTriggerPCChargeTutorial += TriggerPCChargeTutorial;
        EventsManager.OnTriggerPCInteractTutorial += TriggerPCInteractTutorial;
        EventsManager.OnResetPCTutorial += ResetPCTutorial;
        EventsManager.OnTutorialTeleport += DisableTutorials;
    }

    private void OnDisable()
    {
       EventsManager.OnCreatedPCPlayer += GetPCPlayerData;
       EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyData;
       EventsManager.OnCreatedVRPlayer -= GetVRPlayerData; 

       EventsManager.OnTriggerPCChargeTutorial -= TriggerPCChargeTutorial;
       EventsManager.OnTriggerPCInteractTutorial -= TriggerPCInteractTutorial;
       EventsManager.OnResetPCTutorial -= ResetPCTutorial;
       EventsManager.OnTutorialTeleport -= DisableTutorials;
    }

    private void GetPCPlayerData()
    {
        BeginPCTransformation();
    }
    
    private void GetPCPlayerBodyData()
    {
        EventsManager.TriggerNarratorAudio("PCIntro", 0, false);
        ToggleTutorialRpc(0);
        // StartCoroutine(DelayPCSpawn());
    }

    private void TriggerPCChargeTutorial()
    {
        if(!_hasSeenChargeTutorial)
        {
            _hasSeenChargeTutorial = true;
            EventsManager.TriggerNarratorAudio("PCIntro", 1, false);
        }

        ToggleTutorialRpc(1);
    }

    private void TriggerPCInteractTutorial()
    {
        if(!_hasSeenInteractTutorial)
        {
            _hasSeenInteractTutorial = true;
            EventsManager.TriggerNarratorAudio("PCIntro", 2, false);
        }

        ToggleTutorialRpc(2);
    }

    private void ResetPCTutorial()
    {
        EventsManager.TriggerNarratorAudio("PCIntro", 0, false);
        ToggleTutorialRpc(0);

        _hasSeenChargeTutorial = false;
        _hasSeenInteractTutorial = false;
    }

    private void DisableTutorials()
    {
        ToggleTutorialRpc(-1);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleTutorialRpc(int _tutorialNum)
    {
        switch(_tutorialNum)
        {
            case -1:
                ToggleTutorial(null);
                break;
            case 0:
                ToggleTutorial(pcMovementTutorial);
                break;
            case 1:
                ToggleTutorial(pcChargeTutorial);
                break;
            case 2:
                ToggleTutorial(pcInteractTutorial);
                break;

        }
    }

    private void ToggleTutorial(GameObject tutorial)
    {
        pcMovementTutorial.SetActive(false);
        pcChargeTutorial.SetActive(false);
        pcInteractTutorial.SetActive(false);
        
        if(tutorial != null) tutorial.SetActive(true);
    }

    public void BeginPCTransformation()
    {
        EventsManager.TriggerNarratorAudio("VRIntro", 2, true);
        StartCoroutine(DelayPCSpawn());
    }

    IEnumerator DelayPCSpawn()
    {
        pcPipes[visiblePCPipes].SetActive(true);
        visiblePCPipes++;

        yield return new WaitForSeconds(pcSpawnDelay);

        if(visiblePCPipes >= pcPipes.Length)
        {
            // Destroy(pcStatue);
            EventsManager.AddPCPlayerBody();
        } else {
            StartCoroutine(DelayPCSpawn());
        }
    }

    private void GetVRPlayerData()
    {
        StartCoroutine(VRIntroEvent());
    }

    IEnumerator VRIntroEvent()
    {
        EventsManager.TriggerNarratorAudio("VRIntro", 0, true);

        yield return new WaitForSeconds(12f);

        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
            {
                vrPlayerCamera = child;
                vrPlayerCameraCover = vrPlayerCamera.GetChild(0).GetComponent<Renderer>().material;
                StartCoroutine(FadeVRPlayerIntoGame(2f));
                break;
            }
        }
    }

    private void CheckedRoofEvent()
    {
        EventsManager.TriggerNarratorAudio("VRIntro", 1, true);
    }

    IEnumerator FadeVRPlayerIntoGame(float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            Color colour = vrPlayerCameraCover.color;
            colour.a = (_delay - elapsed) / _delay;
            vrPlayerCameraCover.color = colour;

            yield return null;
        }

        _constantlyCheck = true;
        _checkRoof = true;
    }

    private void Update()
    {
        if(!_constantlyCheck) return;

        if(_checkRoof)
        {
            if(IsLookingAtRoof())
            {
                _constantlyCheck = false;
                _checkRoof = false;
                CheckedRoofEvent();
            }
        }
    }

    bool IsLookingAtRoof()
    {
        Vector3 _positionDifference = roof.position - vrPlayerCamera.position;
        float _spotLightAngle = Vector3.Angle(vrPlayerCamera.forward, _positionDifference);

        //Returns false if the roof is outside of the Vr Player's sight (the angle remains unchanged)
        if (_spotLightAngle > 55 * 0.5f) return false;
        return true;
    }
}
