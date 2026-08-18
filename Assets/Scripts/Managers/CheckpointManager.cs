using UnityEngine;

using System;
using System.Collections;
using Unity.Netcode;

using UnityEngine.SceneManagement;
using System.IO;

public class CheckpointManager : NetworkBehaviour
{
    [Header("Checkpoints")]
    [SerializeField] private Transform tutorialCheckpoint;
    [SerializeField] private Transform firstCheckpoint;
    [SerializeField] private Transform secondCheckpoint;
    [SerializeField] private Transform thirdCheckpoint;
    [SerializeField] private Transform fourthCheckpoint;
    private Transform currentCheckpoint;

    private GameObject _pcPlayer;
    private bool _canPCFunction;

    private bool _hasCompleteTutorial;
    private bool _hasUsedTeleporter;
    private bool _hasCollectedEverything;
    private bool _hasCrossedCrookedBridges;

    [SerializeField] private Transform hiddenButtons;
    [SerializeField] private Transform hiddenArrows;

    private NetworkVariable<int> nextScene = new (-1);

    private void OnEnable()
    {
        currentCheckpoint = tutorialCheckpoint;

        EventsManager.OnTutorialTeleport += CompleteTutorial;
        EventsManager.OnIncreaseChanceOfSpawningEnemy += UsedTeleporter;
        EventsManager.OnEverythingCollected += EverythingCollected;
        EventsManager.OnCrossedCrookedBridges += CrossedCrookedBridges;

        nextScene.OnValueChanged += OnNextSceneChanged;
    }

    private void OnDisable()
    {
        EventsManager.OnTutorialTeleport -= CompleteTutorial;
        EventsManager.OnIncreaseChanceOfSpawningEnemy -= UsedTeleporter;
        EventsManager.OnEverythingCollected -= EverythingCollected;
        EventsManager.OnCrossedCrookedBridges -= CrossedCrookedBridges;

        nextScene.OnValueChanged -= OnNextSceneChanged;
    }

    private void GetPCPlayerData()
    {
        if( GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer");   
            _canPCFunction = true;
        }
    }

    //If the PC Player completes the tutorial, and starts the game, 
    // the first checkpoint will be set as the active checkpoint
    private void CompleteTutorial()
    {
        if(!_hasCompleteTutorial)
        {
            ActivateFirstCheckpointClientRpc();
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

    //This activates the first checkpoint
    [ClientRpc]
    public void ActivateFirstCheckpointClientRpc()
    {
        _hasCompleteTutorial = true;
        currentCheckpoint = firstCheckpoint;
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
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void RespawnPCPlayerRpc()
    {
        if(!_canPCFunction) GetPCPlayerData();

        if(_canPCFunction) 
        {
            EventsManager.DestroyAllEnemies();

            //If the PC Player has not completed the tutorial, then tutorial audio cues are reset
            if(!_hasCompleteTutorial)
            {
                EventsManager.ResetPCTutorial();
            }
            
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

    //Used by UI Button to change the scene
    public void PlayLevelClient(string _sceneName)
    {
        SetNextSceneServerRpc(GetSceneIndex(_sceneName));
    }

    //Gets the correct scene number
    private int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);

            if (name == sceneName) return i;
        }

        return -1;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetNextSceneServerRpc(int _newScene)
    {
        nextScene.Value = _newScene;
    }

    private void OnNextSceneChanged(int previousValue, int newValue)
    {
        PlayLevelServerRpc(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(newValue)));
    }

    //Loads the correct scene
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayLevelServerRpc(string _sceneName)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }
}
