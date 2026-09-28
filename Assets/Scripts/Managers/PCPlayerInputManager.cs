using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Unity.Netcode;

//This script controls all input
public class PCPlayerInputManager : NetworkBehaviour
{
    private PlayerInput playerInput;

    [SerializeField] private static bool canMove = true;
    private InputAction moveAction;

    [SerializeField] private static bool canJump = true;
    private InputAction jumpAction;

    [SerializeField] private static bool canInteract = true;
    private InputAction interactAction;

    [SerializeField] private static bool canFire = true;
    private InputAction fireAction;

    [SerializeField] private static bool canFire2 = true;
    private InputAction fire2Action;

    [SerializeField] private static bool canCancel = true;
    private InputAction cancelAction;

    private bool _isInNetwork;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("No PlayerInput component found in the scene!");
                return;
            }
        }

        canMove = true;
        moveAction = playerInput.actions["Move"];

        canJump = true;
        jumpAction = playerInput.actions["Jump"];

        canInteract = true;
        interactAction = playerInput.actions["Interact"];

        canFire = true;
        fireAction = playerInput.actions["Fire"];

        canFire2 = true;
        fire2Action = playerInput.actions["Fire2"];

        canCancel = true;
        cancelAction = playerInput.actions["Cancel"];

        
    }

    private void OnEnable()
    {
        EventsManager.OnEnteredTemple += FreezePlayer;
        EventsManager.OnTogglePCPlayerMovement += ToggleAll;
        // EventsManager.OnFreezePCPlayer += FreezePlayer;

        EventsManager.OnToggleAll += ToggleAll;
        EventsManager.OnToggleRestriction += ToggleRestriction;

        moveAction.Enable();
        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMove;

        jumpAction.Enable();
        jumpAction.performed += HandleJump;

        interactAction.Enable();
        interactAction.performed += HandleInteract;

        fireAction.Enable();
        fireAction.performed += HandleFire;
        fireAction.canceled += HandleFire;

        fire2Action.Enable();
        fire2Action.performed += HandleFire2;

        cancelAction.Enable();
        cancelAction.performed += HandleCancel;
    }

    private void OnDisable()
    {
        EventsManager.OnEnteredTemple -= FreezePlayer;
        EventsManager.OnTogglePCPlayerMovement -= ToggleAll;
        // EventsManager.OnFreezePCPlayer -= FreezePlayer;

        EventsManager.OnToggleAll -= ToggleAll;
        EventsManager.OnToggleRestriction -= ToggleRestriction;

        moveAction.performed -= HandleMove;
        moveAction.canceled -= HandleMove;
        moveAction.Disable();

        jumpAction.performed -= HandleJump;
        jumpAction.Disable();

        interactAction.performed -= HandleInteract;
        interactAction.Disable();

        fireAction.performed -= HandleFire;
        fireAction.canceled -= HandleFire;
        fireAction.Disable();

        fire2Action.performed -= HandleFire2;
        fire2Action.Disable();

        cancelAction.performed -= HandleCancel;
        cancelAction.Disable();
    }

    public override void OnNetworkSpawn()
    {
        _isInNetwork = true;
    }

    private void FreezePlayer(bool _toggle)
    {
        Debug.Log("FreezePlayer: " + _toggle);
        ToggleAll(!_toggle);
    }

    private void ToggleAll(bool _toggle)
    {
        Debug.Log("ToggleAll: " + _toggle);
        ToggleRestriction("All", _toggle);
    }

    //Toggles the restriction of various input controls
    private void ToggleRestriction(string _restriction, bool _toggle)
    {
        Debug.Log("ToggleRestriction: " + _restriction + " : " + _toggle);
        if(_restriction == "Move" || _restriction == "All")
        {
            canMove = _toggle;
            if (_toggle == false)
            {
                Vector2 move = Vector2.zero;
                EventsManager.Move(move);
            }
        }
        
        if(_restriction == "Jump" || _restriction == "All") canJump = _toggle;

        if(_restriction == "Interact" || _restriction == "All") canInteract = _toggle;
        
        if(_restriction == "Fire" || _restriction == "All") canFire = _toggle;
        
        if(_restriction == "Fire2" || _restriction == "All") canFire2 = _toggle;
        
        if(_restriction == "Cancel" || _restriction == "All") canCancel = _toggle;
    }

    //Controls the movement controls of the PC Player
    private void HandleMove(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            Vector2 move = ctx.ReadValue<Vector2>();
            EventsManager.Move(move);
        }
    }

    //Controls the jump controls of the PC Player
    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (canJump) EventsManager.Jump();
    }

    //Controls the interaction controls of the PC Player
    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        if (canInteract) EventsManager.Interact();
    }

    //Controls the fire controls (recharges the PC flashlight) of the PC Player
    private void HandleFire(InputAction.CallbackContext ctx)
    {
        if (canFire) EventsManager.Fire(ctx);
    }

    //Controls the fire2 controls (toggles the PC flashlight) of the PC Player
    private void HandleFire2(InputAction.CallbackContext ctx)
    {
        if (canFire2) EventsManager.Fire2();
    }

    //Controls the cancel controls (toggles the pause menu) of the PC Player
    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        if (canCancel && _isInNetwork) EventsManager.Cancel(true);

        // if (canFire2) EventsManager.Fire2();
    }

    public void HandleCancelUsingUIButtons()
    {
        if (canCancel && _isInNetwork) EventsManager.Cancel(true);
    }
}
