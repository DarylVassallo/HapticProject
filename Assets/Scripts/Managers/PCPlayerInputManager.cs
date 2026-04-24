using UnityEngine;
using UnityEngine.InputSystem;
using System;
//This script controls all input
public class PCPlayerInputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public static event Action<string, bool> OnToggleRestriction;

    public static event Action<Vector2> OnMove;
    [SerializeField] private static bool canMove = true;
    private InputAction moveAction;

    public static event Action OnJump;
    [SerializeField] private static bool canJump = true;
    private InputAction jumpAction;

    public static event Action OnCrouch;
    [SerializeField] private static bool canCrouch = true;
    private InputAction crouchAction;

    public static event Action<InputAction.CallbackContext> OnSprint;
    [SerializeField] private static bool canSprint = true;
    private InputAction sprintAction;

    public static event Action<InputAction.CallbackContext> OnFire;
    [SerializeField] private static bool canFire = true;
    private InputAction fireAction;

    public static event Action OnFire2;
    [SerializeField] private static bool canFire2 = true;
    private InputAction fire2Action;

    public static event Action OnCancel;
    [SerializeField] private static bool canCancel = true;
    private InputAction cancelAction;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("No PlayerInput component found in the scene!");
                return;
            }
        }

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];
        sprintAction = playerInput.actions["Sprint"];
        fireAction = playerInput.actions["Fire"];
        fire2Action = playerInput.actions["Fire2"];
        cancelAction = playerInput.actions["Cancel"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMove;

        jumpAction.Enable();
        jumpAction.performed += HandleJump;

        crouchAction.Enable();
        crouchAction.performed += HandleCrouch;

        sprintAction.Enable();
        sprintAction.performed += HandleSprint;
        sprintAction.canceled += HandleSprint;

        fireAction.Enable();
        fireAction.performed += HandleFire;
        fireAction.canceled += HandleFire;

        fire2Action.Enable();
        fire2Action.performed += HandleFire2;

        cancelAction.Enable();
        cancelAction.performed += HandleCancel;

        MenuManager.OnToggleAll += ToggleAll;
    }

    private void OnDisable()
    {
        moveAction.performed -= HandleMove;
        moveAction.canceled -= HandleMove;
        moveAction.Disable();

        jumpAction.performed -= HandleJump;
        jumpAction.Disable();

        crouchAction.performed -= HandleCrouch;
        crouchAction.Disable();

        sprintAction.performed -= HandleSprint;
        sprintAction.canceled -= HandleSprint;
        sprintAction.Disable();

        fireAction.performed -= HandleFire;
        fireAction.canceled -= HandleFire;
        fireAction.Disable();

        fire2Action.performed -= HandleFire2;
        fire2Action.Disable();

        cancelAction.performed -= HandleCancel;
        cancelAction.Disable();

        MenuManager.OnToggleAll -= ToggleAll;
    }

    private void ToggleAll(bool toggle)
    {
        ToggleRestriction("All", toggle);
    }

    public static void ToggleRestriction(string restriction, bool toggle)
    {
        if(restriction == "Move" || restriction == "All")
        {
            canMove = toggle;
            if (toggle == false)
            {
                Vector2 move = Vector2.zero;
                OnMove?.Invoke(move);
            }
        }
        
        if(restriction == "Jump" || restriction == "All")
        {
            canJump = toggle;
        }
        
        if(restriction == "Crouch" || restriction == "All")
        {
            canCrouch = toggle;
        }
        
        if(restriction == "Sprint" || restriction == "All")
        {
            canSprint = toggle;
        }
        
        if(restriction == "Fire" || restriction == "All")
        {
            canFire = toggle;
        }
        
        if(restriction == "Fire2" || restriction == "All")
        {
            canFire2 = toggle;
        }
        
        if(restriction == "Cancel" || restriction == "All")
        {
            canCancel = toggle;
        }
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            Vector2 move = ctx.ReadValue<Vector2>();
            OnMove?.Invoke(move);
        }
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (canJump)
        {
            OnJump?.Invoke();
        }
    }

    private void HandleCrouch(InputAction.CallbackContext ctx)
    {
        if (canCrouch)
        {
            OnCrouch?.Invoke();
        }
    }

    private void HandleSprint(InputAction.CallbackContext ctx)
    {
        if (canSprint)
        {
            OnSprint?.Invoke(ctx);
        }
    }

    private void HandleFire(InputAction.CallbackContext ctx)
    {
        if (canFire)
        {
            OnFire?.Invoke(ctx);
        }
    }

    private void HandleFire2(InputAction.CallbackContext ctx)
    {
        if (canFire2)
        {
            OnFire2?.Invoke();
        }
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        if (canCancel)
        {
            OnCancel?.Invoke();
        }
    }
}
