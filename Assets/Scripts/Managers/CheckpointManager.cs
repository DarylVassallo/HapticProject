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

    private GameObject _pcPlayer;
    private bool _canPCFunction;

    private bool _hasUsedTeleporter;
    private bool _hasCollectedEverything;
    private bool _hasCrossedCrookedBridges;

    [SerializeField] private Transform hiddenButtons;
    [SerializeField] private Transform hiddenArrows;

    private void OnEnable()
    {
        currentCheckpoint = firstCheckpoint;

        EventsManager.OnIncreaseChanceOfSpawningEnemy += UsedTeleporter;
        EventsManager.OnEverythingCollected += EverythingCollected;
        EventsManager.OnCrossedCrookedBridges += CrossedCrookedBridges;
    }

    private void OnDisable()
    {
        EventsManager.OnIncreaseChanceOfSpawningEnemy -= UsedTeleporter;
        EventsManager.OnEverythingCollected -= EverythingCollected;
        EventsManager.OnCrossedCrookedBridges -= CrossedCrookedBridges;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer");   
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
        if(!_hasCollectedEverything) ActivateThirdCheckpointClientRpc();
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

        EventsManager.DestroyAllEnemies();
        EventsManager.DisableEnemySpawning();

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
                EventsManager.ActivateSecondCheckpointHiddenObjects();
                break;
            case 3:
                EventsManager.ActivateThirdCheckpointHiddenObjects();
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
            EventsManager.DestroyAllEnemies();

            //If the PC Player has not reached a teleport pad, then progress made with the switches are reset
            if(!_hasUsedTeleporter)
            {
                EventsManager.ResetHiddenSwitches();
            }

            //If the PC Player has not collected all the collectables, then all collectables and teleport pads are reset
            if(!_hasCollectedEverything)
            {
                EventsManager.ResetHiddenButtons();
                EventsManager.ResetTeleportPads();
            }

            //The PC Player's health and position are reset
            EventsManager.ResetHealth(_pcPlayer);
            _pcPlayer.transform.position = currentCheckpoint.position;
        }
    }
}
