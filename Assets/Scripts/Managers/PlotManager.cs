using UnityEngine;

using System.Collections;

public class PlotManager : MonoBehaviour
{
    private bool _constantlyCheck;
    private bool _checkRoof;

    [Header("VR Head")]
    private Transform vrPlayerCamera;
    private Material vrPlayerCameraCover;

    [Header("PC Statue")]
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
        DisableTutorials();

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
        // BeginPCTransformation();
    }
    
    private void GetPCPlayerBodyData()
    {
        EventsManager.TriggerNarratorAudio("PCIntro", 0, false);
        ToggleTutorial(pcMovementTutorial);
        // StartCoroutine(DelayPCSpawn());
    }

    private void TriggerPCChargeTutorial()
    {
        if(!_hasSeenChargeTutorial)
        {
            _hasSeenChargeTutorial = true;
            EventsManager.TriggerNarratorAudio("PCIntro", 1, false);
        }

        ToggleTutorial(pcChargeTutorial);
    }

    private void TriggerPCInteractTutorial()
    {
        if(!_hasSeenInteractTutorial)
        {
            _hasSeenInteractTutorial = true;
            EventsManager.TriggerNarratorAudio("PCIntro", 2, false);
        }

        ToggleTutorial(pcInteractTutorial);
    }

    private void ResetPCTutorial()
    {
        EventsManager.TriggerNarratorAudio("PCIntro", 0, false);
        ToggleTutorial(pcMovementTutorial);

        _hasSeenChargeTutorial = false;
        _hasSeenInteractTutorial = false;
    }

    private void DisableTutorials()
    {
        ToggleTutorial(null);
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
            Destroy(pcStatue);
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
