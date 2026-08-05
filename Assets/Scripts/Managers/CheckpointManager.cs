using UnityEngine;

using System;
using System.Collections;
using Unity.Netcode;

public class CheckpointManager : NetworkBehaviour
{
    [Header("Checkpoints")]
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

    private void OnEnable()
    {
        currentCheckpoint = firstCheckpoint;

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

    //If the PC Player reaches a teleport pad for the first time, 
    // the second checkpoint will be set as the active checkpoint
    private void UsedTeleporter(float _chance)
    {
        if(!_hasUsedTeleporter)
        {
            ActivateSecondCheckpointClientRpc();
        }
    }

    //If the PC Player has collected all the collectables, 
    // the third checkpoint will be set as the active checkpoint
    private void EverythingCollected()
    {
        if(!_hasCollectedEverything)
        {
            ActivateThirdCheckpointClientRpc();
        }
    }

    //If the PC Player has crossed the crooked bridges, 
    // the fourth checkpoint will be set as the active checkpoint
    private void CrossedCrookedBridges()
    {
        if(!_hasCrossedCrookedBridges)
        {
            ActivateFourthCheckpointClientRpc();
        }
    }

    //Once the second checkpoint is activated, 
    // all the collectable and hidden arrows are activated
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

    //Once the third checkpoint is activated, 
    // all enemies are removed and are unable to spawn
    [ClientRpc]
    public void ActivateThirdCheckpointClientRpc()
    {
        _hasCollectedEverything = true;
        currentCheckpoint = thirdCheckpoint;

        OnDestroyAllEnemies?.Invoke();
        OnDisableEnemySpawning?.Invoke();

        StartCoroutine(ActivateCheckpoint(3));
    }

    //This activates the fourth checkpoint
    [ClientRpc]
    public void ActivateFourthCheckpointClientRpc()
    {
        _hasCrossedCrookedBridges = true;
        currentCheckpoint = fourthCheckpoint;
    }

    //Once the checkpoint has been set, 
    // an event is invoked to inform other scripts that depend on this
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

    //If the PC Player dies, this sets them to their latest checkpoint, 
    // and reset's various objects depending on the progress made
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RespawnPCPlayerRpc()
    {
        if(!_canPCFunction) GetPCPlayerData();

        if(_canPCFunction) 
        {
            OnDestroyAllEnemies?.Invoke();

            //If the PC Player has not reached a teleport pad, then progress made with the switches are reset
            if(!_hasUsedTeleporter)
            {
                OnResetHiddenSwitches?.Invoke();
            }

            //If the PC Player has not collected all the collectables, then all collectables and teleport pads are reset
            if(!_hasCollectedEverything)
            {
                OnResetHiddenButtons?.Invoke();
                OnResetTeleportPads?.Invoke();
            }

            //The PC Player's health and position are reset
            _pcPlayerHealth.ResetHealth();
            _pcPlayerTransform.position = currentCheckpoint.position;
        }
    }
}
