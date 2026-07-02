using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

public class TeleportManager : NetworkBehaviour
{
    [SerializeField] private Renderer[] collectableIndicators;
    private int _collectablePoints = 0;

    [SerializeField] private Material collectedMaterial;

    [SerializeField] private Material[] maps;
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

    private List<Transform> _barList;

    private float _timeLimit;
    private float _timePassed;
    
    private void OnEnable()
    {
        _barList = new List<Transform>();

        HiddenTeleportButtonInteract.OnTriggerHiddenButton += GainHiddenButton;
        _currentMap.OnValueChanged += ChangeMap;

        Debug.Log("OnEnable TeleportPad.AddNewBar");
        TeleportPad.OnAddNewBar += AddNewBar;
    }

    private void OnDisable()
    {
        HiddenTeleportButtonInteract.OnTriggerHiddenButton -= GainHiddenButton;
        _currentMap.OnValueChanged -= ChangeMap;

        TeleportPad.OnAddNewBar -= AddNewBar;
    }

    public override void OnNetworkSpawn()
    {
        CurrentMapServerRpc();
        mapRenderer.material = maps[_currentMap.Value];
    }

    private void AddNewBar(Transform newBar)
    {
        Debug.Log("AddNewBar: " + newBar);
        _barList.Add(newBar);
    }


    [ServerRpc(RequireOwnership = false)]
    public void CurrentMapServerRpc()
    {
        if(_canChangeMap.Value)
        {
            _currentMap.Value = UnityEngine.Random.Range(0, maps.Length);
            _canChangeMap.Value = false;

            for(int i = 0; i < teleportConnections[_currentMap.Value].teleportPairs.Length; i++)
            {
                teleportConnections[_currentMap.Value].teleportPairs[i].
                    entrancePad.GetComponent<TeleportPad>().
                    ExitPadServerRpc(teleportConnections[_currentMap.Value].teleportPairs[i].exitPad.position);
            }
            
            _timeLimit = UnityEngine.Random.Range(minDelay, maxDelay);
            _timePassed = 0;
            StartCoroutine(ChangeMapDelay(_timeLimit));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CanChangeMapServerRpc(bool canChange)
    {
        _canChangeMap.Value = canChange;
    }

    private void ChangeMap(int previous, int current)
    {
        mapRenderer.material = maps[current];
    }

    IEnumerator ChangeMapDelay(float delay)
    {
        float elapsed = 0f;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / delay;

            for(int i = 0; i < _barList.Count; i++)
            {
                _barList[i].localScale = new Vector3(   _barList[i].localScale.x, 
                                                    _barList[i].localScale.y, 
                                                    Mathf.Lerp(0.95f, 0f, t));
            }

            yield return null;
        }

        CanChangeMapServerRpc(true);
        CurrentMapServerRpc();

        for(int i = 0; i < _barList.Count; i++)
        {
            _barList[i].localScale = new Vector3(   _barList[i].localScale.x, 
                                                _barList[i].localScale.y, 
                                                0.95f);
        }
    }

    private void GainHiddenButton()
    {
        collectableIndicators[_collectablePoints].material.SetColor("_BaseColor", collectedMaterial.GetColor("_BaseColor"));
        _collectablePoints++;
    }
}
