using UnityEngine;
using System.Collections;

using Unity.Netcode;
public class PlotManager : NetworkBehaviour
{
    private bool _constantlyCheck;
    private bool _checkRoof;
    private bool _skipTutorial;

    [Header("VR Head")]
    private Transform vrPlayerCamera;
    private Material vrPlayerCameraCover;

    [Header("PC Statue Pipes")]
    [SerializeField] private GameObject pcStatue;
    [SerializeField] private GameObject[] pcPipes;
    private int visiblePCPipes = 0;
    [SerializeField] private float pcSpawnDelay;
    private bool isPCTransforming;

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
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyData;
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData;

        EventsManager.OnTriggerPCChargeTutorial += TriggerPCChargeTutorial;
        EventsManager.OnTriggerPCInteractTutorial += TriggerPCInteractTutorial;
        EventsManager.OnResetPCTutorial += ResetPCTutorial;
        EventsManager.OnTutorialTeleport += DisableTutorials;

        EventsManager.OnUsingOnlyPCPlayer += UseOnlyPCPlayer;
        EventsManager.OnSkipTutorial += SkipTutorial;
        
        EventsManager.OnReachedSwitches += ReachedSwitches;
        EventsManager.OnFirstEnemyCreated += FirstEnemyCreated;
        EventsManager.OnFirstActiveInteractiveObject += FirstActiveInteractiveObject;

        EventsManager.OnReachedFirstTeleporter += ReachedFirstTeleporter;
        EventsManager.OnFirstCollectable += FirstCollectable;

        EventsManager.OnUsedCrookedBridgeTeleporter += UsedFirstCrookedBridgeTeleporter;
    }

    private void OnDisable()
    {
       EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyData;
       EventsManager.OnCreatedVRPlayer -= GetVRPlayerData; 

       EventsManager.OnTriggerPCChargeTutorial -= TriggerPCChargeTutorial;
       EventsManager.OnTriggerPCInteractTutorial -= TriggerPCInteractTutorial;
       EventsManager.OnResetPCTutorial -= ResetPCTutorial;
       EventsManager.OnTutorialTeleport -= DisableTutorials;

       EventsManager.OnUsingOnlyPCPlayer -= UseOnlyPCPlayer;
       EventsManager.OnSkipTutorial -= SkipTutorial;

       EventsManager.OnReachedSwitches -= ReachedSwitches;
       EventsManager.OnFirstEnemyCreated -= FirstEnemyCreated;
       EventsManager.OnFirstActiveInteractiveObject -= FirstActiveInteractiveObject;

       EventsManager.OnReachedFirstTeleporter -= ReachedFirstTeleporter;
       EventsManager.OnFirstCollectable -= FirstCollectable;

        EventsManager.OnUsedCrookedBridgeTeleporter += UsedFirstCrookedBridgeTeleporter;
    }

    private void SkipTutorial()
    {
        _skipTutorial = true;
    }

    private void UseOnlyPCPlayer()
    {
        BeginPCTransformation();
    }
    
    private void GetPCPlayerBodyData()
    {
        Debug.Log("PCIntro IntroducePC");
        EventsManager.TriggerNarratorAudio("PCIntro", "IntroducePC", false, -1, true);
        ToggleTutorialRpc(0);
        // StartCoroutine(DelayPCSpawn());
    }

    private void TriggerPCChargeTutorial()
    {
        // if(IsOwner) EventsManager.ChangeHealthForEntity(GameObject.FindGameObjectWithTag("PCPlayer"), -25);

        if(!_hasSeenChargeTutorial)
        {
            _hasSeenChargeTutorial = true;

            Debug.Log("PCIntro IntroduceCharge");
            EventsManager.TriggerNarratorAudio("PCIntro", "IntroduceCharge", false, -1, true);
            EventsManager.OnNarratorStopped += PCClimb;
        }

        ToggleTutorialRpc(1);
    }

    private void PCClimb()
    {
        Debug.Log("PCIntro PCClimb");
        EventsManager.TriggerNarratorAudio("PCIntro", "IntroduceClimb", false, 1, true);
        EventsManager.OnNarratorStopped -= PCClimb;
    }

    private void TriggerPCInteractTutorial()
    {
        // if(IsOwner) EventsManager.ChangeHealthForEntity(GameObject.FindGameObjectWithTag("PCPlayer"), 25);

        if(!_hasSeenInteractTutorial)
        {
            _hasSeenInteractTutorial = true;

            Debug.Log("PCIntro IntroduceInteraction");
            EventsManager.TriggerNarratorAudio("PCIntro", "IntroduceInteraction", false, -1, true);
        }

        ToggleTutorialRpc(2);
    }

    private void ResetPCTutorial()
    {
        Debug.Log("PCIntro IntroducePC");
        EventsManager.TriggerNarratorAudio("PCIntro", "IntroducePC", false, -1, true);
        ToggleTutorialRpc(0);

        _hasSeenChargeTutorial = false;
        _hasSeenInteractTutorial = false;
    }

    private void DisableTutorials()
    {
        ToggleTutorialRpc(-1);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
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
        if(isPCTransforming) return;

        isPCTransforming = true;
        
        Debug.Log("VRIntro WaitForPC");
        EventsManager.TriggerNarratorAudio("VRIntro", "WaitForPC", true, -1, true);
        StartCoroutine(DelayPCSpawn());
    }

    IEnumerator DelayPCSpawn()
    {
        pcPipes[visiblePCPipes].SetActive(true);
        visiblePCPipes++;

        yield return new WaitForSeconds(pcSpawnDelay);

        if(visiblePCPipes >= pcPipes.Length || _skipTutorial)
        {
            // Destroy(pcStatue);
            EventsManager.AddPCPlayerBody();
        } else {
            StartCoroutine(DelayPCSpawn());
        }
    }

    private void ReachedSwitches()
    {
        Debug.Log("PCSwitchSection IntroduceSwitchPC");
        EventsManager.TriggerNarratorAudio("PCSwitchSection", "IntroduceSwitchPC", false, -1, false);
    }

    private void FirstEnemyCreated()
    {
        Debug.Log("PCSwitchSection FirstEnemyCreated");
        EventsManager.TriggerNarratorAudio("PCSwitchSection", "FirstEnemyCreated", false, -1, false);
    }

    private void FirstActiveInteractiveObject()
    {
        Debug.Log("VRSwitchSection FirstActiveInteractiveObject");
        EventsManager.TriggerNarratorAudio("VRSwitchSection", "FirstActiveInteractiveObject", true, -1, false);
    }

    private void ReachedFirstTeleporter()
    {
        Debug.Log("PCTeleporter ReachedFirstTeleporter");
        EventsManager.TriggerNarratorAudio("PCTeleporter", "ReachedFirstTeleporter", false, -1, false);
    }
    private void FirstCollectable()
    {
        Debug.Log("VRTeleporter FirstCollectable");
        EventsManager.TriggerNarratorAudio("VRTeleporter", "FirstCollectable", true, -1, false);
    }

    private void UsedFirstCrookedBridgeTeleporter()
    {
        Debug.Log("PCCrookedBridge ReachedCrookedBridge");
        EventsManager.TriggerNarratorAudio("PCCrookedBridge", "ReachedCrookedBridge", false, -1, false);
    }
    private void CrookedBridgePCPlayerWaited()
    {
        Debug.Log("VRCrookedBridge PCPlayerWaited");
        EventsManager.TriggerNarratorAudio("VRCrookedBridge", "PCPlayerWaited", true, -1, false);
    }

    private void ReachedRotatingBridges()
    {
        Debug.Log("PCRotatingBridges ReachedRotatingBridges");
       EventsManager.TriggerNarratorAudio("PCRotatingBridges", "ReachedRotatingBridges", false, -1, false);
    }

    private void PCReachedEnd()
    {
        Debug.Log("PCReachedEnd ReachedEnd");
       EventsManager.TriggerNarratorAudio("PCReachedEnd", "ReachedEnd", false, -1, false);
    }
    private void VRReachedEnd()
    {
        Debug.Log("VRReachedEnd ReachedEnd");
        EventsManager.TriggerNarratorAudio("VRReachedEnd", "ReachedEnd", true, -1, false);
    }
    

    private void GetVRPlayerData()
    {
        if(_skipTutorial)
        {
            foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
                {
                    vrPlayerCamera = child;
                    vrPlayerCameraCover = vrPlayerCamera.GetChild(0).GetComponent<Renderer>().material;

                    Color colour = vrPlayerCameraCover.color;
                    colour.a = 0;
                    vrPlayerCameraCover.color = colour;
                    break;
                }
            }

            UncoverVRPlayer();
        }
        else
        {
            VRIntroEvent();
        }
    }

    private void GetPCPlayerData()
    {
        EventsManager.AddPCPlayerBody();
    }

    private void VRIntroEvent()
    {
        Debug.Log("VRIntro IntroduceVR");
        EventsManager.TriggerNarratorAudio("VRIntro", "IntroduceVR", true, -1, true);
        EventsManager.OnNarratorStopped += UncoverVRPlayer;
    }

    private void UncoverVRPlayer()
    {
        Debug.Log("VRIntro CheckRoof");
        EventsManager.TriggerNarratorAudio("VRIntro", "CheckRoof", true, 0, true);
        EventsManager.OnNarratorStopped -= UncoverVRPlayer;

        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
            {
                vrPlayerCamera = child;
                vrPlayerCameraCover = vrPlayerCamera.GetChild(0).GetComponent<Renderer>().material;

                if(_skipTutorial)
                {
                    Color colour = vrPlayerCameraCover.color;
                    colour.a = 0;
                    vrPlayerCameraCover.color = colour;
                }

                StartCoroutine(FadeVRPlayerIntoGame(2f));
                break;
            }
        }
    }

    private void CheckedRoofEvent()
    {
        Debug.Log("VRIntro GrabPCStatue");
        EventsManager.TriggerNarratorAudio("VRIntro", "GrabPCStatue", true, -1, true);
    }

    IEnumerator FadeVRPlayerIntoGame(float _delay)
    {
        float elapsed = 0f;

        Color colour = vrPlayerCameraCover.color;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            if(_skipTutorial)
            {
                colour = vrPlayerCameraCover.color;
                colour.a = 0;
                vrPlayerCameraCover.color = colour;

                elapsed = 999f;
            }else{
                colour = vrPlayerCameraCover.color;
                colour.a = (_delay - elapsed) / _delay;
                vrPlayerCameraCover.color = colour;
            }            

            yield return null;
        }
        
        colour.a = 0;
        vrPlayerCameraCover.color = colour;

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
