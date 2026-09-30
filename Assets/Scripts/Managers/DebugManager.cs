using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class DebugManager : NetworkBehaviour
{

    [Header("Network Debugging")]
    [SerializeField] private bool isUsingPlayMode;
    [SerializeField] private bool isUsingOnlyVRPlayer;
    [SerializeField] private bool isUsingOnlyPCPlayer;

    [Header("Tutorial Debugging")]
    [SerializeField] private bool skipTutorial;

    [Header("Player Icon Debugging")]
    [SerializeField] private bool startWithPlayerIcon;

    [Header("Maze Debugging")]
    [SerializeField] private bool startWithActivatedSpinWheel;
    [SerializeField] private bool startWithActivatedHealthBall;
    [SerializeField] private bool startWithActivatedDefenseButton;

    [Header("Enemy Debugging")]
    [SerializeField] private bool disableEnemies;

    [Header("Teleport Debugging")]
    [SerializeField] private bool startWithInstantTeleport;
    [SerializeField] private bool disableTeleportChange;

    [Header("Collectable Debugging")]
    [SerializeField] private bool requireOnlyOneCollectable;

    private void OnEnable()
    {
        StartCoroutine(DelayDebugging());
    }

    public override void OnNetworkSpawn()
    {
        if(startWithActivatedSpinWheel) EventsManager.ActivateSpinWheel();
        if(startWithActivatedHealthBall) EventsManager.ActivateHealthBall();
        if(startWithActivatedDefenseButton) EventsManager.ActivateDefenseButton();
    }

    IEnumerator DelayDebugging()
    {
        yield return new WaitForSeconds(0.01f);

        if(isUsingPlayMode) EventsManager.UsingPlayMode();
        if(isUsingOnlyVRPlayer) EventsManager.UsingOnlyVRPlayer();
        if(isUsingOnlyPCPlayer) EventsManager.UsingOnlyPCPlayer();

        if(skipTutorial) EventsManager.SkipTutorial();

        if(startWithPlayerIcon) EventsManager.StartWithPlayerIcon();

        // if(startWithActivatedSpinWheel) EventsManager.ActivateSpinWheel();
        // if(startWithActivatedHealthBall) EventsManager.ActivateHealthBall();
        // if(startWithActivatedDefenseButton) EventsManager.ActivateDefenseButton();

        if(disableEnemies) EventsManager.DisableEnemies();
        
        if(startWithInstantTeleport) EventsManager.StartWithInstantTeleport();
        if(disableTeleportChange) EventsManager.DisableTeleportChange();

        if(requireOnlyOneCollectable) EventsManager.RequireOnlyOneCollectable();
    }
}
