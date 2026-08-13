using UnityEngine;

using System.Collections;

public class PlotManager : MonoBehaviour
{
    [Header("VR Cover")]
    [SerializeField] private float coverIncrement = 0.0001f;
    private bool uncoverVRCamera = false;
    private Material vrPlayerCameraCover;

    [Header("PC Statue")]
    [SerializeField] private GameObject pcStatue;
    [SerializeField] private GameObject[] pcPipes;
    private int visiblePCPipes = 0;
    [SerializeField] private float pcSpawnDelay;

    [Header("PC Tutorial UI")]
    [SerializeField] private GameObject pcMovementTutorial;
    [SerializeField] private GameObject pcChargeTutorial;
    [SerializeField] private GameObject pcInteractTutorial;
    

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
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData;

        EventsManager.OnTriggerPCChargeTutorial += TriggerPCChargeTutorial;
        EventsManager.OnTriggerPCInteractTutorial += TriggerPCInteractTutorial;
    }

    private void OnDisable()
    {
       EventsManager.OnCreatedPCPlayer -= GetPCPlayerData;
       EventsManager.OnCreatedVRPlayer -= GetVRPlayerData; 

       EventsManager.OnTriggerPCChargeTutorial -= TriggerPCChargeTutorial;
       EventsManager.OnTriggerPCInteractTutorial -= TriggerPCInteractTutorial;
    }

    private void GetPCPlayerData()
    {
        ToggleTutorial(pcMovementTutorial);
        StartCoroutine(DelayPCSpawn());
    }

    private void TriggerPCChargeTutorial()
    {
        ToggleTutorial(pcChargeTutorial);
    }

    private void TriggerPCInteractTutorial()
    {
        ToggleTutorial(pcInteractTutorial);
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
        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
            {
                vrPlayerCameraCover = child.GetChild(0).GetComponent<Renderer>().material;
                FadeVRPlayerIntoGame();
                break;
            }
        }
    }

    private void FadeVRPlayerIntoGame()
    {
        Color colour = vrPlayerCameraCover.color;
        colour.a = colour.a - coverIncrement;
        vrPlayerCameraCover.color = colour;

        if(colour.a <= coverIncrement)
        {
            uncoverVRCamera = false;
        }
        else
        {
            uncoverVRCamera = true;
        }
    }

    private void Update()
    {
       if(uncoverVRCamera) FadeVRPlayerIntoGame();
    }
}
