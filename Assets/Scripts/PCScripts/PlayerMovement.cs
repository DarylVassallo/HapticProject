using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Cinemachine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//This script allows the PC Player to move and jump (modified to use input actions from an input manager script).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class PlayerMovement : NetworkBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 5f;

    [Header("Jump and Fall")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -12f;
    private bool _isGravityEnabled;
    [SerializeField] private float initialFallVelocity = -2f;


    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform xAxisBodyTransform;
    private CinemachineInputAxisController _inputAxisController;

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _prevIsGrounded;
    private float _verticalVelocity;

    [SerializeField] private float bobSpeed = 7f;
    [SerializeField] private float bobAmount = 0.2f;
    private float healthBobAmount = 1;
    private float totalBobTimer = 0f;

    private Animator _animator;
    private Transform pcFlashlight;

    private NetworkVariable<bool> isWalking = new(false);
    private NetworkVariable<bool> isJumping = new(false);
    private NetworkVariable<float> bodyYRotation = new(0f);
    private NetworkVariable<float> bodyXRotation = new(0f);

    private void Awake()
    {
        _isGravityEnabled = true;

        _characterController = GetComponent<CharacterController>();

        // CinemachineCore.GetInputAxis = HandleAxisInput;

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
    
    private void OnEnable()
    {
        EventsManager.OnEnteredTemple += FreezePlayerCompletely;
        EventsManager.OnToggleAll += TogglePlayerCompletely;
        EventsManager.OnTogglePCPlayerGravity += TogglePlayerGravity;

        EventsManager.OnMove += ChangeMotion;
        EventsManager.OnJump += Jump;

        EventsManager.OnChangeHealthCamera += ChangeHealthCamera;

        isWalking.OnValueChanged += ChangeWalkingAnimation;
        isJumping.OnValueChanged += ChangeJumpingAnimation;
        bodyYRotation.OnValueChanged += SetBodyYRotation;
        bodyXRotation.OnValueChanged += SetBodyXRotation;
    }

    private void OnDisable()
    {
        EventsManager.OnEnteredTemple -= FreezePlayerCompletely;
        EventsManager.OnToggleAll -= TogglePlayerCompletely;
        EventsManager.OnTogglePCPlayerGravity -= TogglePlayerGravity;

        EventsManager.OnMove -= ChangeMotion;
        EventsManager.OnJump -= Jump;

        EventsManager.OnChangeHealthCamera -= ChangeHealthCamera;

        isWalking.OnValueChanged -= ChangeWalkingAnimation;
        isJumping.OnValueChanged -= ChangeJumpingAnimation;
        bodyXRotation.OnValueChanged -= SetBodyXRotation;
    }

    private void FreezePlayerCompletely(bool _freeze)
    {
        TogglePlayerCompletely(!_freeze);
    }

    private void TogglePlayerCompletely(bool _toggle)
    {
        ToggleAllPlayerCameraMotion(_toggle);
        TogglePlayerGravity(_toggle);
    }
    private void ToggleAllPlayerCameraMotion(bool _toggle)
    {
        _inputAxisController.enabled = _toggle;
    }
    private void TogglePlayerGravity(bool _toggle)
    {        
        _isGravityEnabled = _toggle;

        if(!_isGravityEnabled)
        {
            _characterController.Move(new Vector3(0, 0, 0) * Time.deltaTime);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsWalkingServerRpc(bool _walk)
    {
        isWalking.Value = _walk;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsJumpingServerRpc(bool _jump)
    {
        isJumping.Value = _jump;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetBodyYRotationServerRpc(float _newYRotation)
    {
        bodyYRotation.Value = _newYRotation;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetBodyXRotationServerRpc(float _newXRotation)
    {
        bodyXRotation.Value = _newXRotation;
    }

    private void ChangeWalkingAnimation(bool previous, bool current)
    {
        _animator.SetBool("IsWalking", current);
    }

    private void ChangeJumpingAnimation(bool previous, bool current)
    {
        _animator.SetBool("IsJumping", current);
    }

    private void SetBodyYRotation(float previous, float current)
    {
        bodyTransform.rotation = Quaternion.Euler(0, current + 90f, 0);
    }

    private void SetBodyXRotation(float previous, float current)
    {
        Vector3 eulerRotation = xAxisBodyTransform.eulerAngles;
        xAxisBodyTransform.rotation = Quaternion.Euler(current, eulerRotation.y, eulerRotation.z);
    }

    //Changes the PC Player's motion based on the input
    private void ChangeMotion(Vector2 input)
    {
        if(!IsOwner) return;

        _moveInput = input;
    }

    private void FixedUpdate()
    {
        if (!IsOwner)   return;

        _isGrounded = _characterController.isGrounded;

        if(!_isGrounded && _prevIsGrounded)
        {
            SetIsJumpingServerRpc(true);
        }else if(_isGrounded && !_prevIsGrounded)
        {
            SetIsJumpingServerRpc(false);
        }

        HandleGravity();
        HandleMovement();

        _prevIsGrounded = _isGrounded;
    }

    //Applies the jump force if on the ground
    private void Jump()
    {
        if(!IsOwner) return;

        if(_isGrounded)
        {
            _verticalVelocity = jumpForce;
        }
    }

    //Changes the amount of head bobbing (how much the camera shakes during motion) based on the amount of health (the lower the health the more the head bobbing)
    private void ChangeHealthCamera(float _currentHealth)
    {
        healthBobAmount = _currentHealth / 100f;
    }

    //Controls the gravity applied to the PC Player
    private void HandleGravity()
    {
        if(_isGravityEnabled)
        {
            if(_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = initialFallVelocity;
            }

            _verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            _verticalVelocity = 0;
        }
    }


    private void HandleMovement()
    {
        var move = cameraTransform.TransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y)).normalized;
        SetBodyYRotationServerRpc(cameraTransform.eulerAngles.y);
        SetBodyXRotationServerRpc(cameraTransform.eulerAngles.x);
        
        //If moving, the head bobbing effect will apply
        if(move != new Vector3(0, 0, 0))
        {
            SetIsWalkingServerRpc(true);

            //Used ChatGPT to generate initial bobbing logic
            totalBobTimer += Time.deltaTime * bobSpeed;
            cameraTransform.parent.transform.localPosition = new Vector3(   cameraTransform.parent.transform.localPosition.x, 
                                                                            Mathf.Sin(totalBobTimer) * (bobAmount * (1f - healthBobAmount)), 
                                                                            cameraTransform.parent.transform.localPosition.z);
        }
        else
        {
            SetIsWalkingServerRpc(false);
        }
        
        //Calculates the final movement velocity
        var finalMove = move * walkSpeed;

        finalMove.y = _verticalVelocity;

        var collisions = _characterController.Move(finalMove * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0)
        {
            _verticalVelocity = initialFallVelocity;
        }
    }
}
