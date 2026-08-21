using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
public class TeleportManager : NetworkBehaviour
{
    [SerializeField] private Renderer[] collectableIndicators;
    private int _collectablePoints = 0;
    private bool _isEverythingCollected;

    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material deactiveMaterial;

    [SerializeField] private Material[] maps;
    [SerializeField] private Material finalMap;

    [SerializeField] private Renderer mapRenderer;
    private NetworkVariable<int> _currentMap = new (-1);
    private NetworkVariable<bool> _canChangeMap = new (true);

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

    private void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }
    
    private void OnEnable()
    {
        _barList = new List<Transform>();

        EventsManager.OnTriggerHiddenButton += GainCollectable;
        EventsManager.OnResetHiddenButtons += RemoveCollectable;
        _currentMap.OnValueChanged += ChangeMap;

        EventsManager.OnAddNewBar += AddNewBar;

        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;

        EventsManager.OnTutorialTeleport += TutorialTeleportRpc;

        EventsManager.OnDisableTeleportChange += DisableTeleportChange;
    }

    private void OnDisable()
    {
        EventsManager.OnTriggerHiddenButton -= GainCollectable;
        EventsManager.OnResetHiddenButtons -= RemoveCollectable;
        _currentMap.OnValueChanged -= ChangeMap;

        EventsManager.OnAddNewBar -= AddNewBar;

        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;

        EventsManager.OnTutorialTeleport -= TutorialTeleportRpc;

        EventsManager.OnDisableTeleportChange -= DisableTeleportChange;
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
            CurrentMapServerRpc(false);
            _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TutorialTeleportRpc()
    {        
        EventsManager.TogglePCTrigger(false);

        if(tutorialTeleportCount < tutorialTeleportPads.Length)
        {        
            EventsManager.FreezePCPlayer(true);

            if(!IsOwner)
            {
                _pcPlayerTransform.position = tutorialTeleportPads[tutorialTeleportCount].position;
                _pcPlayerTransform.GetChild(0).rotation = tutorialTeleportPads[tutorialTeleportCount].rotation;
            }

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
        
        if(tutorialTeleportCount >= tutorialTeleportPads.Length)
        {
            _audioSource.Stop();

            EventsManager.TogglePCTrigger(true);
            EventsManager.FreezePCPlayer(false);
            // EventsManager.ToggleRestriction("Move", true);

            EventsManager.FixTeleportEffect(0, true);
        }
    }

    IEnumerator TutorialTeleportDelay(float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            if(tutorialTeleportCount >= tutorialTeleportPads.Length && elapsed >= _delay/2)
            {
                EventsManager.FixTeleportEffect(0, true);
            } else {
                EventsManager.FixTeleportEffect(1 - Mathf.Sin(elapsed / _delay * Mathf.PI), false);
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
    public void CurrentMapServerRpc(bool _foundAllCollectables)
    {
        //If the PC Player has found all the collectables, then the final map and teleport connections are set
        if(_foundAllCollectables)
        {
            _currentMap.Value = 999;
        }

        //If not all collectables have been found, then a random map is chosen to replace the current one
        if(_canChangeMap.Value)
        {
            if(!_foundAllCollectables)
            {
                _currentMap.Value = UnityEngine.Random.Range(0, maps.Length);
                Debug.Log("_currentMap.Value: " + _currentMap.Value);
                mapRenderer.material = maps[_currentMap.Value];
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void CanChangeMapServerRpc(bool canChange)
    {
        _canChangeMap.Value = canChange;
    }

    private void ChangeMap(int previous, int current)
    {
        if(!IsOwner) return;

        EventsManager.ClearTeleportNumPad();

        Debug.Log("ChangeMap current: " + current);

        //If the final map is called, the final teleport connections are set up
        if(current == 999)
        {
            mapRenderer.material = finalMap;
            CanChangeMapServerRpc(false);

            for(int i = 0; i < finalTeleportConnections.teleportPairs.Length; i++)
            {
                EventsManager.SetExitPadTransform(  finalTeleportConnections.teleportPairs[i].entrancePad.gameObject, 
                                                    finalTeleportConnections.teleportPairs[i].exitPad);
            }

        //This sets up the correct map for the VR Player, and set up the correct teleport connections.
        // It also sets up a random time limit before the next time the map changes again
        } else {
            mapRenderer.material = maps[current];
            CanChangeMapServerRpc(false);

            for(int i = 0; i < teleportConnections[_currentMap.Value].teleportPairs.Length; i++)
            {
                SetExitPadRpc(i);   
            }
            
            _timeLimit = UnityEngine.Random.Range(minDelay, maxDelay);

            Debug.Log("Teleport Delay: " + _timeLimit);

            if(IsOwner) StartCoroutine(ChangeMapDelay(_timeLimit));
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetExitPadRpc(int _index)
    {
        Debug.Log("EntrancePad: " + teleportConnections[_currentMap.Value].teleportPairs[_index].entrancePad.gameObject);
        Debug.Log("ExitPad: " +  teleportConnections[_currentMap.Value].teleportPairs[_index].exitPad.gameObject);
        EventsManager.SetExitPadTransform(  teleportConnections[_currentMap.Value].teleportPairs[_index].entrancePad.gameObject, 
                                                    teleportConnections[_currentMap.Value].teleportPairs[_index].exitPad);
    }

    IEnumerator ChangeMapDelay(float delay)
    {
        float elapsed = 0f;

        //The bar of every teleport pad is slowly reduced, to represent the amount of time left before the map is changed 
        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / delay;

            if(!_isEverythingCollected)
            {
                ChangeBarSizeRpc(t); 
            }

            yield return null;
        }

        //This changes the current map
        CanChangeMapServerRpc(true);
        CurrentMapServerRpc(false);

        //The teleport bars are reset to their full size
        ChangeBarSizeRpc(0.95f); 
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeBarSizeRpc(float _newSize)
    {
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
            CurrentMapServerRpc(true);
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
