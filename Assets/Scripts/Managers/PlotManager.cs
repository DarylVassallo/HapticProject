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
    [SerializeField] private Transform pcStatueBubble;
    private Material _pcStatueBubbleMaterial;
    [SerializeField] private Transform pcSpawnBubble;
    private Material _pcSpawnBubbleMaterial;

    [Header("PC Tutorial UI")]
    [SerializeField] private GameObject pcMovementTutorial;
    [SerializeField] private GameObject pcChargeTutorial;
    private bool _hasSeenChargeTutorial;
    [SerializeField] private GameObject pcInteractTutorial;
    private bool _hasSeenInteractTutorial;

    [Header("Roof")]
    [SerializeField] private Transform roof;

    [Header("VRWalls")]
    [SerializeField] private Animator _vrWallsAnimator;

    private bool _usingOnlyVRPlayer;
    private bool _createdVRPlayer;
    private bool _createdPCPlayer;
    

    private void Awake()
    {
        _vrWallsAnimator.speed = 0;

        ToggleTutorial(null);

        for(int i = 0; i < pcPipes.Length; i++)
        {
            pcPipes[i].SetActive(false);
        }

        _pcStatueBubbleMaterial = pcStatueBubble.GetComponent<Renderer>().material;
        _pcSpawnBubbleMaterial = pcSpawnBubble.GetComponent<Renderer>().material;
    }
    
    private void OnEnable()
    {
        EventsManager.OnUsingOnlyVRPlayer += UsingOnlyVRPlayer;

        EventsManager.OnCreatedVRPlayer += GetVRPlayerData;
        EventsManager.OnCreatedPCPlayer += GetPCPlayerData;
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyData;

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
        EventsManager.OnCrookedBridgePCPlayerWaited += CrookedBridgePCPlayerWaited;

        EventsManager.OnCrossedCrookedBridges += ReachedRotatingBridges;

        EventsManager.OnPCReachedEnd += PCReachedEnd;
        EventsManager.OnEnteredTemple += VRReachedEnd;
    }

    private void OnDisable()
    {
        EventsManager.OnUsingOnlyVRPlayer -= UsingOnlyVRPlayer;

        EventsManager.OnCreatedVRPlayer -= GetVRPlayerData; 
        EventsManager.OnCreatedPCPlayer += GetPCPlayerData;
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyData;

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

        EventsManager.OnUsedCrookedBridgeTeleporter -= UsedFirstCrookedBridgeTeleporter;
        EventsManager.OnCrookedBridgePCPlayerWaited -= CrookedBridgePCPlayerWaited;

        EventsManager.OnCrossedCrookedBridges -= ReachedRotatingBridges;

        EventsManager.OnPCReachedEnd -= PCReachedEnd;
        EventsManager.OnEnteredTemple -= VRReachedEnd;
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
        EventsManager.TogglePCTrigger(true);
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
            // EventsManager.AddPCPlayerBody();
            StartCoroutine(SwitchStatueToPCPlayer(1f));
        } else {
            StartCoroutine(DelayPCSpawn());
        }
    }

    IEnumerator SwitchStatueToPCPlayer(float _delay)
    {
        float elapsed = 0f;
        Color colour;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
                
            float _newScale = Mathf.Lerp(
                10f,
                4f,
                1 - Mathf.Sin(elapsed / _delay * Mathf.PI)
            );
            pcStatueBubble.localScale = new Vector3(_newScale, _newScale, _newScale);
            pcSpawnBubble.localScale = new Vector3(_newScale, _newScale, _newScale);

            float _newAlpha = Mathf.Lerp(
                0f,
                1f,
                1 - Mathf.Sin(elapsed / _delay * Mathf.PI)
            );

            colour = _pcStatueBubbleMaterial.color;
            colour.g = 1 - _newAlpha;
            colour.a = _newAlpha;
            _pcStatueBubbleMaterial.color = colour;
            _pcSpawnBubbleMaterial.color = colour;

            yield return null;
        }

        colour = _pcStatueBubbleMaterial.color;
        colour.a = 0;
        _pcStatueBubbleMaterial.color = colour;
        _pcSpawnBubbleMaterial.color = colour;

        EventsManager.AddPCPlayerBody();
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
    private void VRReachedEnd(bool _entered)
    {
        Debug.Log("VRReachedEnd ReachedEnd");
        EventsManager.TriggerNarratorAudio("VRReachedEnd", "ReachedEnd", true, -1, false);

        _vrWallsAnimator.SetBool("IsEnding", true);
        _vrWallsAnimator.speed = 1f;
        StartCoroutine(BeginRotateVRArea(0.75f));
    }

    IEnumerator BeginRotateVRArea(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        StartCoroutine(RotateVRArea(5f));
    }

    IEnumerator RotateVRArea(float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            _vrWallsAnimator.speed = 2 * (elapsed / _delay);  

            yield return null;
        }

        StartCoroutine(FadeVRPlayerAway(5f));
    }
    
    private void UsingOnlyVRPlayer()
    {
        _usingOnlyVRPlayer = true;
        if(_usingOnlyVRPlayer && _createdVRPlayer) VRIntroEvent();
    }

    private void GetVRPlayerData()
    {
        _createdVRPlayer = true;

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

        if(_usingOnlyVRPlayer && _createdVRPlayer) VRIntroEvent();
    }

    private void GetPCPlayerData()
    {
        _createdPCPlayer = true;
        if(_createdVRPlayer && _createdPCPlayer) VRIntroEvent();
    }

    // private void GetPCPlayerData()
    // {
    //     Debug.Log("GetPCPlayerData");
    //     EventsManager.AddPCPlayerBody();
    // }

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
            Debug.Log("child 1: " + child);
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
            {
                Debug.Log("child 2: " + child);

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
        Debug.Log("FadeVRPlayerIntoGame");
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

    IEnumerator FadeVRPlayerAway(float _delay)
    {
        float elapsed = 0f;

        Color colour = Color.white;
        colour.a = 0;
        vrPlayerCameraCover.color = colour;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            colour = vrPlayerCameraCover.color;
            colour.a = elapsed / _delay;
            vrPlayerCameraCover.color = colour;          

            yield return null;
        }
        
        colour.a = 1;
        vrPlayerCameraCover.color = colour;

        EventsManager.WinGame(true);
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
