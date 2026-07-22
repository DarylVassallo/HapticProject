using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//This script allows the PC Player to move, sprint, jump, and crouch (modified to using input actions from an input manager script).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class PlayerMovement : NetworkBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump and Fall")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [Header("Crouching")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private float cameraOffset = 0.4f;


    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;
    private CinemachineInputAxisController _inputAxisController;

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _isCrouching;
    private float _verticalVelocity;
    private float _targetHeight;

    [SerializeField] private float bobSpeed = 7f;
    [SerializeField] private float bobAmount = 0.2f;
    private float healthBobAmount = 1;
    private float totalBobTimer = 0f;

    private Animator _animator;
    private Transform pcFlashlight;

    private NetworkVariable<bool> isWalking = new(false);
    private NetworkVariable<float> bodyYRotation = new(0f);

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _targetHeight = standingHeight;

        CinemachineCore.GetInputAxis = HandleAxisInput;

        _inputAxisController = cameraTransform.GetComponent<CinemachineInputAxisController>();

        _animator = GetComponentInChildren<Animator>();

        foreach (Transform child in this.gameObject.transform)
        {
            foreach (Transform grandChild in child)
            {
                foreach (Transform greatGrandChild in grandChild)
                {
                    if (greatGrandChild.CompareTag("PCFlashLight"))
                    {
                        pcFlashlight = greatGrandChild;
                        break;
                    }
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // _animator.gameObject.SetActive(true);    
            
            if (pcFlashlight != null)
            {
                foreach (MeshRenderer mesh in pcFlashlight.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh.enabled = false;
                }
            }

            cameraTransform.GetComponent<CinemachineCamera>().enabled = false;
            return;
        }
        else
        {
            // _animator.gameObject.SetActive(true); 
            // _animator.gameObject.SetActive(false); 

            if (pcFlashlight != null)
            {
                foreach (MeshRenderer mesh in pcFlashlight.GetComponentsInChildren<MeshRenderer>())
                {
                    mesh.enabled = true;
                }
            }

            cameraTransform.GetComponent<CinemachineCamera>().enabled = true;
        }
    }

    private float HandleAxisInput(string axisName)
    {
        Debug.Log("HandleAxisInput");
        // if (axisName == "Mouse X")
        //     return Mouse.current.delta.x.ReadValue();

        // if (axisName == "Mouse Y")
        //     return Mouse.current.delta.y.ReadValue();

        return 0;
    }
    
    private void OnEnable()
    {
        PCPlayerInputManager.OnMove += ChangeMotion;
        PCPlayerInputManager.OnJump += Jump;
        PCPlayerInputManager.OnCrouch += Crouch;
        PCPlayerInputManager.OnSprint += Sprint;

        Health.OnChangeHealthCamera += ChangeHealthCamera;

        isWalking.OnValueChanged += ChangeWalkingAnimation;
        bodyYRotation.OnValueChanged += SetBodyYRotation;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnMove -= ChangeMotion;
        PCPlayerInputManager.OnJump -= Jump;
        PCPlayerInputManager.OnCrouch -= Crouch;
        PCPlayerInputManager.OnSprint -= Sprint;

        Health.OnChangeHealthCamera -= ChangeHealthCamera;

        isWalking.OnValueChanged -= ChangeWalkingAnimation;
        bodyYRotation.OnValueChanged -= SetBodyYRotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIsWalkingServerRpc(bool _walk)
    {
        isWalking.Value = _walk;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBodyYRotationServerRpc(float _newYRotation)
    {
        bodyYRotation.Value = _newYRotation;
    }

    private void ChangeWalkingAnimation(bool previous, bool current)
    {
        _animator.SetBool("IsWalking", current);
    }

    private void SetBodyYRotation(float previous, float current)
    {
        bodyTransform.rotation = Quaternion.Euler(0, current, 0);
    }

    private void ChangeMotion(Vector2 input)
    {
        if(!IsOwner) return;

        _moveInput = input;
    }
    private void FixedUpdate()
    {
        if (!IsOwner)   return;

        _isGrounded = _characterController.isGrounded;
        HandleGravity();
        HandleMovement();
        HandleCrouchTransition();
    }

    private void Jump()
    {
        if(!IsOwner) return;

        if(_isGrounded)
        {
            _verticalVelocity = jumpForce;
        }
    }

    private void Crouch()
    {
        if(!IsOwner) return;

        if (_isCrouching)
        {
            if (!CanStandUp())
            {
                return;
            }
            _targetHeight = standingHeight;
        }
        else
        {
            _targetHeight = crouchingHeight;
        }
        _isCrouching = !_isCrouching;
    }

    private bool CanStandUp()
    {
        return !Physics.CapsuleCast(
            transform.position + _characterController.center,
            transform.position + (Vector3.up * _characterController.height / 2),
            _characterController.radius,
            Vector3.up
        );
    }
    private void Sprint(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;

        _isRunning = context.performed;
    }

    private void ChangeHealthCamera(float _currentHealth)
    {
        healthBobAmount = _currentHealth / 100f;
    }

    private void HandleGravity()
    {
        if(_isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = initialFallVelocity;
        }

        _verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        var move = cameraTransform.TransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y)).normalized;
        SetBodyYRotationServerRpc(cameraTransform.eulerAngles.y);
        // bodyTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        
        if(move != new Vector3(0, 0, 0))
        {
            // _animator.SetBool("IsWalking", true);
            SetIsWalkingServerRpc(true);

            //Used ChatGPT to generate initial bobbing logic
            totalBobTimer += Time.deltaTime * bobSpeed;
            cameraTransform.parent.transform.localPosition = new Vector3(   cameraTransform.parent.transform.localPosition.x, 
                                                                            Mathf.Sin(totalBobTimer) * (bobAmount * (1f - healthBobAmount)), 
                                                                            cameraTransform.parent.transform.localPosition.z);
        }
        else
        {
            // _animator.SetBool("IsWalking", false);
            SetIsWalkingServerRpc(false);
        }
        
        var currentSpeed = _isCrouching ? crouchSpeed : _isRunning ? runSpeed : walkSpeed;
        var finalMove = move * currentSpeed;

        finalMove.y = _verticalVelocity;

        var collisions = _characterController.Move(finalMove * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0)
        {
            _verticalVelocity = initialFallVelocity;
        }
    }

    private void HandleCrouchTransition()
    {
        var currentHeight = _characterController.height;
        if (Mathf.Abs(currentHeight - _targetHeight) < 0.01f)
        {
            _characterController.height = _targetHeight;
            return;
        }
        
        var newHeight = Mathf.Lerp(currentHeight, _targetHeight, crouchTransitionSpeed * Time.deltaTime);

        _characterController.height = newHeight;
        _characterController.center = new Vector3(0f, (newHeight * 0.5f) - 1f, 0f);

        var cameraTargetPosition = cameraTransform.localPosition;
        cameraTargetPosition.y = (_targetHeight / 2) - cameraOffset;
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            cameraTargetPosition,
            crouchTransitionSpeed * Time.deltaTime);
    }

}
