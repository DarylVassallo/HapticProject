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
    [SerializeField] private float pcSpawnDelay;
    

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayer += GetPCPlayerData;
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData;
    }

    private void OnDisable()
    {
       EventsManager.OnCreatedVRPlayer -= GetVRPlayerData; 
    }

    private void GetPCPlayerData()
    {
        StartCoroutine(DelayPCSpawn());
    }

    IEnumerator DelayPCSpawn()
    {
        yield return new WaitForSeconds(pcSpawnDelay);
        Destroy(pcStatue);
        EventsManager.AddPCPlayerBody();
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
