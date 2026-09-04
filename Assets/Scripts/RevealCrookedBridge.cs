using UnityEngine;

public class RevealCrookedBridge : MonoBehaviour
{
    private Transform _pcPlayer;
    private bool _pcPlayerNearby;

    private void Awake()
    {
        _pcPlayerNearby = false;
    }
    
    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;
    }

    private void GetPCPlayerBodyDataRpc()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if(_other.transform == _pcPlayer) _pcPlayerNearby = true;
    }

    private void OnTriggerExit(Collider _other)
    {
        if(_other.transform == _pcPlayer) _pcPlayerNearby = false;
    }

    private void Update()
    {
        if(!_pcPlayerNearby) return;

        Color colour = this.GetComponent<Renderer>().material.color;
        Debug.Log(this.gameObject + " : Distance: " + Vector3.Distance(this.transform.position, _pcPlayer.position));
        colour.a = Vector3.Distance(this.transform.position, _pcPlayer.position) / 10f;
        Debug.Log(this.gameObject + " : colour.a: " + colour.a);
        Debug.Log(this.gameObject + "====================");
        this.GetComponent<Renderer>().material.color = colour;
    }
}