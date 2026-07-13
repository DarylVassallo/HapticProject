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

    public bool arePadsReady = true;

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
    [SerializeField] private TeleportConnections[] teleportConnections;
    [SerializeField] private TeleportConnections finalTeleportConnections;

    private List<Transform> _barList;

    private float _timeLimit;
    private float _timePassed;

    public static event Action OnEverythingCollected;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    void Awake()
    {
        _canPCFunction = false;
        _canVRFunction = false;
    }
    
    private void OnEnable()
    {
        _barList = new List<Transform>();

        HiddenTeleportButtonInteract.OnTriggerHiddenButton += GainHiddenButton;
        HiddenTeleportButtonInteract.OnResetHiddenButton += RemoveHiddenButton;
        _currentMap.OnValueChanged += ChangeMap;

        TeleportPad.OnAddNewBar += AddNewBar;

        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;
    }

    private void OnDisable()
    {
        HiddenTeleportButtonInteract.OnTriggerHiddenButton -= GainHiddenButton;
        HiddenTeleportButtonInteract.OnResetHiddenButton -= RemoveHiddenButton;
        _currentMap.OnValueChanged -= ChangeMap;

        TeleportPad.OnAddNewBar -= AddNewBar;

        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;
    }

    // public override void OnNetworkSpawn()
    // {
    //     CurrentMapServerRpc(false);
    //     mapRenderer.material = maps[_currentMap.Value];
    // }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;
            InitialMap();
        }
    }

    private void GetVRPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("VRPlayer") != null)
        {
            _canVRFunction = true;
            InitialMap();
        }
    }

    private void InitialMap()
    {
        if(_canPCFunction && _canVRFunction)
        {
            CurrentMapServerRpc(false);
            mapRenderer.material = maps[_currentMap.Value];
        }
    }

    private void AddNewBar(Transform newBar)
    {
        _barList.Add(newBar);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CurrentMapServerRpc(bool _foundAllCollectables)
    {
        if(_foundAllCollectables)
        {
            _currentMap.Value = 999;
        }

        if(_canChangeMap.Value)
        {
            if(!_foundAllCollectables)
            {
                _currentMap.Value = UnityEngine.Random.Range(0, maps.Length);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CanChangeMapServerRpc(bool canChange)
    {
        _canChangeMap.Value = canChange;
    }

    private void ChangeMap(int previous, int current)
    {
        if(current == 999)
        {
            mapRenderer.material = finalMap;
            CanChangeMapServerRpc(false);

            for(int i = 0; i < finalTeleportConnections.teleportPairs.Length; i++)
            {
                finalTeleportConnections.teleportPairs[i].
                    entrancePad.GetComponent<TeleportPad>().
                    SetExitPadTransform(finalTeleportConnections.teleportPairs[i].exitPad);
            }
        }
        else
        {
            mapRenderer.material = maps[current];
            CanChangeMapServerRpc(false);

            for(int i = 0; i < teleportConnections[_currentMap.Value].teleportPairs.Length; i++)
            {
                teleportConnections[_currentMap.Value].teleportPairs[i].
                    entrancePad.GetComponent<TeleportPad>().
                    SetExitPadTransform(teleportConnections[_currentMap.Value].teleportPairs[i].exitPad);
            }
            
            _timeLimit = UnityEngine.Random.Range(minDelay, maxDelay);
            _timePassed = 0;
            StartCoroutine(ChangeMapDelay(_timeLimit));
        }
    }

    IEnumerator ChangeMapDelay(float delay)
    {
        // if(_isEverythingCollected) return;

        float elapsed = 0f;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / delay;

            if(!_isEverythingCollected)
            {
                for(int i = 0; i < _barList.Count; i++)
                {
                    _barList[i].localScale = new Vector3(   _barList[i].localScale.x, 
                                                        _barList[i].localScale.y, 
                                                        Mathf.Lerp(0.95f, 0f, t));
                }
            }

            yield return null;
        }

        CanChangeMapServerRpc(true);
        CurrentMapServerRpc(false);

        for(int i = 0; i < _barList.Count; i++)
        {
            _barList[i].localScale = new Vector3(   _barList[i].localScale.x, 
                                                _barList[i].localScale.y, 
                                                0.95f);
        }
    }

    private void GainHiddenButton()
    {
        // GainHiddenButtonClientRpc();

        collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", activeMaterial.GetColor("_BaseColor"));
        _collectablePoints++;

        Debug.Log("=================");
        Debug.Log("collectableIndicators.Length: " + collectableIndicators.Length);
        Debug.Log("_collectablePoints: " + _collectablePoints);
        // if(1 <= _collectablePoints)
        if(collectableIndicators.Length <= _collectablePoints)
        {
            Debug.Log("Eveything Is Collected");
            _barList = null;
            _isEverythingCollected = true;
            OnEverythingCollected?.Invoke();
            CurrentMapServerRpc(true);
        }
    }

    // [ClientRpc]
    // public void GainHiddenButtonClientRpc()
    // {
    //     Debug.Log("=================");
    //     Debug.Log("GainHiddenButtonClientRpc");
    //     Debug.Log("before collectableIndicators[_collectablePoints].material.GetColor(_BaseColor): " + collectableIndicators[_collectablePoints].material.GetColor("_BaseColor"));
    //     Debug.Log("before _collectablePoints: " + _collectablePoints);
    //     collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", activeMaterial.GetColor("_BaseColor"));
    //     _collectablePoints++;
    //     Debug.Log("after collectableIndicators[_collectablePoints].material.GetColor(_BaseColor): " + collectableIndicators[_collectablePoints].material.GetColor("_BaseColor"));
    //     Debug.Log("after _collectablePoints: " + _collectablePoints);

    //     if(collectableIndicators.Length <= _collectablePoints)
    //     {
    //         Debug.Log("Eveything Is Collected");
    //         _barList = null;
    //         _isEverythingCollected = true;
    //         OnEverythingCollected?.Invoke();
    //         CurrentMapServerRpc(true);
    //     }
    // }

    private void RemoveHiddenButton()
    {
        // RemoveHiddenButtonClientRpc();

        _collectablePoints--;
        collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", deactiveMaterial.GetColor("_BaseColor"));
    }

    // [ClientRpc]
    // private void RemoveHiddenButtonClientRpc()
    // {
    //     Debug.Log("=================");
    //     Debug.Log("RemoveHiddenButtonClientRpc");
    //     Debug.Log("before collectableIndicators[_collectablePoints].material.GetColor(_BaseColor): " + collectableIndicators[_collectablePoints].material.GetColor("_BaseColor"));
    //     Debug.Log("before _collectablePoints: " + _collectablePoints);
    //     _collectablePoints--;
    //     collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", deactiveMaterial.GetColor("_BaseColor"));
    //     Debug.Log("after collectableIndicators[_collectablePoints].material.GetColor(_BaseColor): " + collectableIndicators[_collectablePoints].material.GetColor("_BaseColor"));
    //     Debug.Log("after _collectablePoints: " + _collectablePoints);
    // }
}
