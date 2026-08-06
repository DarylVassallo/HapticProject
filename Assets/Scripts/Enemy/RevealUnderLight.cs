using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

using Unity.Netcode;

//This script updates the hidden object's shader to render correctly based on the spotlights position, direction, and angle (modified to include range) (modifications to the shader used chatgpt).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class RevealUnderLight : NetworkBehaviour
{
    [SerializeField] private bool isPCInteractable;
    [SerializeField] private bool isVRInteractable;
    [SerializeField] private bool isEffectedByLight;
    [SerializeField] private bool isReversed;

    public static event Action<GameObject, bool, bool, bool, bool> OnAddNewHiddenObject;
    
    [SerializeField] private bool isImmediatelyNeeded;
    [SerializeField] private bool isForSecondCheckpoint;
    [SerializeField] private bool isForThirdCheckpoint;

    private void OnEnable()
    {
        if(isForSecondCheckpoint) CheckpointManager.OnActivateSecondCheckpointHiddenObjects += AddHiddenObject;
        if(isForThirdCheckpoint) CheckpointManager.OnActivateThirdCheckpointHiddenObjects += AddHiddenObject;
    }

    private void OnDisable()
    {
        if(isForSecondCheckpoint) CheckpointManager.OnActivateSecondCheckpointHiddenObjects -= AddHiddenObject;
        if(isForThirdCheckpoint) CheckpointManager.OnActivateThirdCheckpointHiddenObjects -= AddHiddenObject;
    }

    public override void OnNetworkSpawn()
    {
        if(isImmediatelyNeeded) StartCoroutine(RequestNewHiddenObject());
        if(!isImmediatelyNeeded) this.gameObject.SetActive(false);
    }

    //Upon spawning, the object is added to a hidden object list
    IEnumerator RequestNewHiddenObject()
    {
        yield return new WaitForSeconds(0.5f);
        AddHiddenObject();
    }

    private void AddHiddenObject()
    {
        OnAddNewHiddenObject?.Invoke(this.gameObject, isPCInteractable, isVRInteractable, isEffectedByLight, isReversed);
    }
}