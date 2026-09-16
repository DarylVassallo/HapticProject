using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

public class TeleportManager : NetworkBehaviour
{
    private bool _isInNetwork;

    [SerializeField] private Transform bubble;
    private Material _bubbleMaterial;

    [SerializeField] private Renderer[] collectableIndicators;
    private int _collectablePoints = 0;
    private bool _isEverythingCollected;

    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material deactiveMaterial;

    [SerializeField] private Material[] maps;
    [SerializeField] private Material finalMap;

    [SerializeField] private Renderer mapRenderer;
    int _currentMap = -1;

    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;

    [System.Serializable]
    private struct TeleportPairs
    {
        public Transform entrancePad;
        public Transform exitPad;
    }

    [System.Serializable]
    private struct TeleportConnections
    {
        public TeleportPairs[] teleportPairs;
    }
    
    [SerializeField] private Transform[] tutorialTeleportPads;
    private int tutorialTeleportCount = 1;

    [SerializeField] private TeleportConnections[] teleportConnections;
    [SerializeField] private TeleportConnections finalTeleportConnections;

    private List<Transform> _barList;

    private float _timeLimit;

    private Transform _pcPlayerTransform;

    private AudioSource _audioSource;
    [SerializeField] private AudioClip _teleportAudio;

    private bool _isPaused;

    private void Awake()
    {
        _isPaused = false;

        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _bubbleMaterial = bubble.GetComponent<Renderer>().material;
    }
    
    private void OnEnable()
    {
        _barList = new List<Transform>();

        EventsManager.OnTriggerHiddenButton += GainCollectable;
        EventsManager.OnResetHiddenButtons += RemoveCollectable;

        EventsManager.OnAddNewBar += AddNewBar;

        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;

        EventsManager.OnTutorialTeleport += TutorialTeleportRpc;

        EventsManager.OnDisableTeleportChange += DisableTeleportChange;

        EventsManager.OnToggleAll += TogglePause;

        EventsManager.TogglePCTrigger(true);
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerHiddenButton -= GainCollectable;
        EventsManager.OnResetHiddenButtons -= RemoveCollectable;

        EventsManager.OnAddNewBar -= AddNewBar;

        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;

        EventsManager.OnTutorialTeleport -= TutorialTeleportRpc;

        EventsManager.OnDisableTeleportChange -= DisableTeleportChange;

        EventsManager.OnToggleAll -= TogglePause;
    }

    public override void OnNetworkSpawn()
    {
        _isInNetwork = true;
    }

    private void TogglePause(bool _toggle)
    {
        _isPaused = !_toggle;
    }

    private void DisableTeleportChange()
    {
        minDelay *= 1000;
        maxDelay *= 1000;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetPCPlayerBodyDataRpc()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            // CurrentMapServerRpc(false, true);
            _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;

            CurrentMapServerRpc(false, true);
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TutorialTeleportRpc()
    {        
        EventsManager.TogglePCTrigger(false);

        if(tutorialTeleportCount == 1) EventsManager.LookAtPlayer(-1, -1, false);

        if(tutorialTeleportCount < tutorialTeleportPads.Length)
        {        
            EventsManager.FreezePCPlayer(true);

            // if(!IsOwner)
            // {
            _pcPlayerTransform.position = tutorialTeleportPads[tutorialTeleportCount].position;
            _pcPlayerTransform.GetChild(0).rotation = tutorialTeleportPads[tutorialTeleportCount].rotation;

            bubble.position = tutorialTeleportPads[tutorialTeleportCount].position;
            bubble.rotation = tutorialTeleportPads[tutorialTeleportCount].rotation;
            // }

            tutorialTeleportCount++;

            if(tutorialTeleportCount >= tutorialTeleportPads.Length - 1)
            {
                _audioSource.Stop();
                _audioSource.clip = _teleportAudio;
                _audioSource.pitch = 3.5f;
                _audioSource.Play();
                _audioSource.enabled = true; 

                StartCoroutine(TutorialTeleportDelay(4f));
            }
            else
            {
                _audioSource.Stop();
                _audioSource.clip = _teleportAudio;
                _audioSource.pitch = 5f;
                _audioSource.Play();
                _audioSource.enabled = true; 

                StartCoroutine(TutorialTeleportDelay(2f));
            }
        }
        
        if(tutorialTeleportCount == tutorialTeleportPads.Length)
        {
            tutorialTeleportCount++;
            _audioSource.Stop();

            EventsManager.TogglePCTrigger(true);
            EventsManager.FreezePCPlayer(false);
            // EventsManager.ToggleRestriction("Move", true);

            EventsManager.FixTeleportEffect(0, true);

            if(IsOwner) EventsManager.ReachedSwitches();
        }
    }

    IEnumerator TutorialTeleportDelay(float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            if(!_isPaused)
            {
                elapsed += Time.deltaTime;
                
                if(tutorialTeleportCount >= tutorialTeleportPads.Length && elapsed >= _delay/2)
                {
                    EventsManager.FixTeleportEffect(0, true);
                } else {
                    float _newScale = Mathf.Lerp(
                        40f,
                        4f,
                        1 - Mathf.Sin(elapsed / _delay * Mathf.PI)
                    );
                    bubble.localScale = new Vector3(_newScale, _newScale, _newScale);

                    float _newAlpha = Mathf.Lerp(
                        0f,
                        1f,
                        1 - Mathf.Sin(elapsed / _delay * Mathf.PI)
                    );
                    Color colour = _bubbleMaterial.color;
                    colour.g = 1 - _newAlpha;
                    colour.a = _newAlpha;
                    _bubbleMaterial.color = colour;
                    
                    EventsManager.FixTeleportEffect(1 - Mathf.Sin(elapsed / _delay * Mathf.PI), false);
                }
            }

            yield return null;
        }

        if(IsOwner) TutorialTeleportRpc();
    }

    //This adds the bar of a teleport pad to a list
    private void AddNewBar(Transform newBar)
    {
        _barList.Add(newBar);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void CurrentMapServerRpc(bool _foundAllCollectables, bool _canChange)
    {
        //If the PC Player has found all the collectables, then the final map and teleport connections are set
        if(_foundAllCollectables)
        {
            _currentMap = 999;
        }

        //If not all collectables have been found, then a random map is chosen to replace the current one
        if(_canChange)
        {
            if(!_foundAllCollectables)
            {
                _currentMap = UnityEngine.Random.Range(0, maps.Length);
                mapRenderer.material = maps[_currentMap];
                ChangeMapRpc(_currentMap);
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeMapRpc(int _newMap)
    {
        if(!IsOwner) return;

        EventsManager.ClearTeleportNumPad();

        //If the final map is called, the final teleport connections are set up
        if(_newMap == 999)
        {
            mapRenderer.material = finalMap;

            for(int i = 0; i < finalTeleportConnections.teleportPairs.Length; i++)
            {
                EventsManager.SetExitPadTransform(  finalTeleportConnections.teleportPairs[i].entrancePad.gameObject, 
                                                    finalTeleportConnections.teleportPairs[i].exitPad);
            }

        //This sets up the correct map for the VR Player, and set up the correct teleport connections.
        // It also sets up a random time limit before the next time the map changes again
        } else {
            mapRenderer.material = maps[_newMap];

            for(int i = 0; i < teleportConnections[_newMap].teleportPairs.Length; i++)
            {
                if(IsOwner) SetExitPadRpc(i, _newMap);   
            }
            
            if(IsOwner)
            {
                _timeLimit = UnityEngine.Random.Range(minDelay, maxDelay);
                StartCoroutine(ChangeMapDelay(_timeLimit));
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetExitPadRpc(int _index, int _newMap)
    {
        EventsManager.SetExitPadTransform(  teleportConnections[_newMap].teleportPairs[_index].entrancePad.gameObject, 
                                                    teleportConnections[_newMap].teleportPairs[_index].exitPad);
    }

    IEnumerator ChangeMapDelay(float delay)
    {
        float elapsed = 0f;

        //The bar of every teleport pad is slowly reduced, to represent the amount of time left before the map is changed 
        while (elapsed < delay)
        {
            if(!_isPaused)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / delay;

                if(!_isEverythingCollected)
                {
                    if(_isInNetwork) ChangeBarSizeRpc(t); 
                }
            }

            yield return null;
        }

        //This changes the current map
        CurrentMapServerRpc(false, true);

        //The teleport bars are reset to their full size
        if(_isInNetwork) ChangeBarSizeRpc(0.95f); 
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeBarSizeRpc(float _newSize)
    {
        EventsManager.UseTeleportBarHaptic(_newSize);

        for(int i = 0; i < _barList.Count; i++)
        {
            _barList[i].localScale = new Vector3(   _barList[i].localScale.x, 
                                                _barList[i].localScale.y, 
                                                Mathf.Lerp(0.95f, 0f, _newSize));
        }
    }

    private void GainCollectable()
    {
        //Once a collectable is collected, this hids it, and gives the PCPlayer a point
        collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", activeMaterial.GetColor("_BaseColor"));
        _collectablePoints++;

        //If all collectables are collected, then the teleport pad's bars are removed, and an event is called to inform other scripts about the PCPlayer's progress
        if(collectableIndicators.Length <= _collectablePoints)
        {
            _barList = null;
            _isEverythingCollected = true;
            EventsManager.EverythingCollected();
            CurrentMapServerRpc(true, false);
        }
    }

    //This resets the latest collectable (to be used when all collectables need to be reset)
    private void RemoveCollectable()
    {
        if(_collectablePoints > 0)
        {
            _collectablePoints--;
            collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", deactiveMaterial.GetColor("_BaseColor"));
        }
    }
}
