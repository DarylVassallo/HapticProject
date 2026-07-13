using UnityEngine;

using System;
using System.Collections;
using Unity.Netcode;

public class CheckpointManager : NetworkBehaviour
{
    [SerializeField] private Transform firstCheckpoint;
    [SerializeField] private Transform secondCheckpoint;
    [SerializeField] private Transform thirdCheckpoint;
    [SerializeField] private Transform fourthCheckpoint;
    private Transform currentCheckpoint;

    private Transform _pcPlayerTransform;
    private Health _pcPlayerHealth;
    private bool _canPCFunction;

    private bool _hasUsedTeleporter;
    private bool _hasCollectedEverything;
    private bool _hasCrossedCrookedBridges;

    public static event Action OnResetHiddenSwitches;
    public static event Action OnResetHiddenButtons;
    public static event Action OnResetTeleportPads;
    public static event Action OnDestroyAllEnemies;
    public static event Action OnDisableEnemySpawning;

    public static event Action OnActivateSecondCheckpointHiddenObjects;
    public static event Action OnActivateThirdCheckpointHiddenObjects;

    [SerializeField] private Transform hiddenButtons;
    [SerializeField] private Transform hiddenArrows;
    [SerializeField] private Transform hiddenCrookedBridgePieces;

    private void OnEnable()
    {
        currentCheckpoint = firstCheckpoint;
        Debug.Log("1 currentCheckpoint : " + currentCheckpoint);

        TeleportPad.OnIncreaseChanceOfSpawningEnemy += UsedTeleporter;
        TeleportManager.OnEverythingCollected += EverythingCollected;
        TeleportPad.OnCrossedCrookedBridges += CrossedCrookedBridges;
    }

    private void OnDisable()
    {
        TeleportPad.OnIncreaseChanceOfSpawningEnemy -= UsedTeleporter;
        TeleportManager.OnEverythingCollected -= EverythingCollected;
        TeleportPad.OnCrossedCrookedBridges -= CrossedCrookedBridges;
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

    private void CrossedCrookedBridges()
    {
        if(!_hasCrossedCrookedBridges)
        {
            ActivateFourthCheckpointClientRpc();
        }
    }

    [ClientRpc]
    public void ActivateSecondCheckpointClientRpc()
    {
        _hasUsedTeleporter = true;
        currentCheckpoint = secondCheckpoint;
        Debug.Log("2 currentCheckpoint : " + currentCheckpoint);

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
        Debug.Log("3 currentCheckpoint : " + currentCheckpoint);

        for (int i = 0; i < hiddenCrookedBridgePieces.childCount; i++)
        {
            hiddenCrookedBridgePieces.GetChild(i).gameObject.SetActive(true);
        }

        OnDestroyAllEnemies?.Invoke();
        OnDisableEnemySpawning?.Invoke();

        StartCoroutine(ActivateCheckpoint(3));
    }

    [ClientRpc]
    public void ActivateFourthCheckpointClientRpc()
    {
        _hasCrossedCrookedBridges = true;
        currentCheckpoint = fourthCheckpoint;
        Debug.Log("4 currentCheckpoint : " + currentCheckpoint);
    }

    IEnumerator ActivateCheckpoint(int checkpointNum)
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("checkpointNum: " + checkpointNum);

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

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RespawnPCPlayerRpc()
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
