using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour
{

    [Header("Network Debugging")]
    [SerializeField] private bool isUsingPlayMode;
    [SerializeField] private bool isUsingOnlyVRPlayer;
    [SerializeField] private bool isUsingOnlyPCPlayer;

    [Header("Maze Debugging")]
    [SerializeField] private bool startWithActivatedSpinWheel;
    [SerializeField] private bool startWithActivatedHealthBall;
    [SerializeField] private bool startWithActivatedDefenseButton;

    [Header("Enemy Debugging")]
    [SerializeField] private bool disableEnemies;

    [Header("Teleport Debugging")]
    [SerializeField] private bool disableTeleportChange;

    private void OnEnable()
    {
        StartCoroutine(DelayDebugging());
    }

    IEnumerator DelayDebugging()
    {
        yield return new WaitForSeconds(0.01f);

        if(isUsingPlayMode) EventsManager.UsingPlayMode();
        if(isUsingOnlyVRPlayer) EventsManager.UsingOnlyVRPlayer();
        if(isUsingOnlyPCPlayer) EventsManager.UsingOnlyPCPlayer();

        if(startWithActivatedSpinWheel) EventsManager.ActivateSpinWheel();
        if(startWithActivatedHealthBall) EventsManager.ActivateHealthBall();
        if(startWithActivatedDefenseButton) EventsManager.ActivateDefenseButton();

        if(disableEnemies) EventsManager.DisableEnemies();

        if(disableTeleportChange) EventsManager.DisableTeleportChange();
    }
}
