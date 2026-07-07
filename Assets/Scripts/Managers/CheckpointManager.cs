using UnityEngine;

using System;
using Unity.Netcode;

public class CheckpointManager : MonoBehaviour
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
            _hasUsedTeleporter = true;
            currentCheckpoint = secondCheckpoint;
        }
    }

    private void EverythingCollected()
    {
        if(!_hasCollectedEverything)
        {
            _hasCollectedEverything = true;
            currentCheckpoint = thirdCheckpoint;
        }
    }

    [ClientRpc]
    public void RespawnPCPlayerClientRpc()
    {
        Debug.Log("RespawnPCPlayerClientRpc");
        if(!_canPCFunction) GetPCPlayerData();

        if(_canPCFunction) 
        {
            Debug.Log("_canPCFunction: " + _canPCFunction);
            Debug.Log("_hasUsedTeleporter: " + _hasUsedTeleporter);
            Debug.Log("_hasCollectedEverything: " + _hasCollectedEverything);

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
