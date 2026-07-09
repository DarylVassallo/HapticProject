using UnityEngine;

using System;
using System.Collections;
using Unity.Netcode;

public class CheckpointManager : NetworkBehaviour
{
    [SerializeField] private Transform firstCheckpoint;
    [SerializeField] private Transform secondCheckpoint;
    [SerializeField] private Transform thirdCheckpoint;
    private Transform currentCheckpoint;

    private Transform _pcPlayerTransform;
    private Health _pcPlayerHealth;
    private bool _canPCFunction;

    private bool _hasUsedTeleporter;
    private bool _hasCollectedEverything;

    public static event Action OnResetHiddenSwitches;
    public static event Action OnResetHiddenButtons;
    public static event Action OnResetTeleportPads;
    public static event Action OnDestroyAllEnemies;

    public static event Action OnActivateSecondCheckpointHiddenObjects;
    public static event Action OnActivateThirdCheckpointHiddenObjects;

    [SerializeField] private Transform hiddenButtons;
    [SerializeField] private Transform hiddenArrows;

    private void OnEnable()
    {
        currentCheckpoint = firstCheckpoint;

        TeleportPad.OnIncreaseChanceOfSpawningEnemy += UsedTeleporter;
        TeleportManager.OnEveythingCollected += EverythingCollected;
    }

    private void OnDisable()
    {
        TeleportPad.OnIncreaseChanceOfSpawningEnemy -= UsedTeleporter;
        TeleportManager.OnEveythingCollected -= EverythingCollected;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;   
            _pcPlayerHealth = _pcPlayerTransform.GetComponent<Health>();   
            _canPCFunction = true;
        }
    }

    private void UsedTeleporter(float _chance)
    {
        if(!_hasUsedTeleporter)
        {
            ActivateSecondCheckpointClientRpc();
        }
    }

    private void EverythingCollected()
    {
        if(!_hasCollectedEverything)
        {
            ActivateThirdCheckpointClientRpc();
        }
    }

    [ClientRpc]
    public void ActivateSecondCheckpointClientRpc()
    {
        _hasUsedTeleporter = true;
        currentCheckpoint = secondCheckpoint;

        for (int i = 0; i < hiddenButtons.childCount; i++)
        {
            hiddenButtons.GetChild(i).gameObject.SetActive(true);
        }

        for (int i = 0; i < hiddenArrows.childCount; i++)
        {
            hiddenArrows.GetChild(i).gameObject.SetActive(true);
        }
        
        StartCoroutine(ActivateCheckpoint(2));
    }

    [ClientRpc]
    public void ActivateThirdCheckpointClientRpc()
    {
        _hasCollectedEverything = true;
        currentCheckpoint = thirdCheckpoint;

        StartCoroutine(ActivateCheckpoint(3));
    }

    IEnumerator ActivateCheckpoint(int checkpointNum)
    {
        yield return new WaitForSeconds(0.5f);

        switch(checkpointNum)
        {
            case 2:
                OnActivateSecondCheckpointHiddenObjects?.Invoke();
                break;
            case 3:
                OnActivateThirdCheckpointHiddenObjects?.Invoke();
                break;
            
        }        
    }

    [ClientRpc]
    public void RespawnPCPlayerClientRpc()
    {
        if(!_canPCFunction) GetPCPlayerData();

        if(_canPCFunction) 
        {
            OnDestroyAllEnemies?.Invoke();

            if(!_hasUsedTeleporter)
            {
                OnResetHiddenSwitches?.Invoke();
            }

            if(!_hasCollectedEverything)
            {
                OnResetHiddenButtons?.Invoke();
                OnResetTeleportPads?.Invoke();
            }

            _pcPlayerHealth.ResetHealth();
            _pcPlayerTransform.position = currentCheckpoint.position;
        }
    }
}
