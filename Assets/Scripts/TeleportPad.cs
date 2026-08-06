using UnityEngine;
using TMPro;

using Unity.Netcode;
using System;
using System.Collections;

public class TeleportPad : NetworkBehaviour
{
    [SerializeField] private Transform exitTeleportPad;

    private Vector3 _positionDifference;

    private TMP_Text _codeText;
    private NetworkVariable<int> _secretCode = new (0);
    [SerializeField] private int numLimit;

    private NetworkVariable<bool> _isPlayerOnPad = new (false);
    private Transform _pcPlayerTransform;

    private TeleportManager _teleportManager;

    [SerializeField] private Transform bar;

    public static event Action<Transform> OnAddNewBar;
    public static event Action<float> OnIncreaseChanceOfSpawningEnemy;
    public static event Action OnCrossedCrookedBridges;

    private bool arePadsReady = true;

    private bool _hasBeenUsed = false;

    private bool _instantTeleport;

    private bool rotateRings = false;

    [SerializeField] private Transform ring;
    [SerializeField] private Transform reverseRing;
    private float rotateSpeed = 1f;
    [SerializeField] private float rotateIncrement = 1f;
    
    [SerializeField] private float minRotateSpeed;
    [SerializeField] private float maxRotateSpeed;

    private Quaternion restRotation;
    [SerializeField] private float rotationRange;

    [SerializeField] private bool _isFinalPad;

    private AudioSource _audioSource;
    [SerializeField] private AudioClip _teleportAudio;

    public static event Action<float, float> OnChangeTeleportRotateSpeed;

    void Awake()
    {
        _codeText = this.GetComponentInChildren<TMP_Text>();
        _codeText.text = "";

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();

        _instantTeleport = false;

        restRotation = ring.rotation;

        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    void Start()
    {
        OnAddNewBar?.Invoke(bar);
    }

    private void OnEnable()
    {
        TeleportManager.OnSendCodeToTeleportPads += CheckInputtedCode;
        TeleportManager.OnEverythingCollected += ActivateInstantTeleport;
        CheckpointManager.OnResetTeleportPads += ResetTeleportPad;

        _secretCode.OnValueChanged += ChangeCodeText;
    }

    private void OnDisable()
    {
        TeleportManager.OnSendCodeToTeleportPads -= CheckInputtedCode;
        TeleportManager.OnEverythingCollected -= ActivateInstantTeleport;
        CheckpointManager.OnResetTeleportPads -= ResetTeleportPad;

        _secretCode.OnValueChanged -= ChangeCodeText;
    }

    //Resets the teleport pad, so the PC Player can use it 'for the first time' again
    private void ResetTeleportPad()
    {
        _hasBeenUsed = false;
    }

    //This triggers the teleportation sequence immediately without requiring the code
    private void ActivateInstantTeleport()
    {
        bar.parent.gameObject.SetActive(false);
        _instantTeleport = true;
    }

    //This allows the TeleportManager to change the current teleport pad's exit pad
    public void SetExitPadTransform(Transform _newExitPadTransform)
    {
        exitTeleportPad = _newExitPadTransform;
        ChangeSecretCodePadServerRpc();
    }

    //This changes the code to a random number (within the set range)
    [ServerRpc(RequireOwnership = false)]
    public void  ChangeSecretCodePadServerRpc()
    {
        _secretCode.Value = UnityEngine.Random.Range(0, numLimit);
    }

    [ServerRpc(RequireOwnership = false)]
    public void  ChangeIsPlayerOnPadServerRpc(bool _newOnPad)
    {
        _isPlayerOnPad.Value = _newOnPad;
    }

    //Changes the code written on the teleport pad, to the correct code
    private void ChangeCodeText(int _previous, int _current)
    {
        _codeText.text = "" + _current + "";
    }

    //If the inputted code is correct, then the teleportation sequence can begin
    private void CheckInputtedCode(int _inputtedCode)
    {
        if(_inputtedCode == _secretCode.Value && _isPlayerOnPad.Value)
        {
            StartRingRotationRpc();
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        //If the PC Player has entered this teleport pad for the first time, the chances for an enemy to randomly spawn increases slightly
        if (!_hasBeenUsed)
        {
            _hasBeenUsed = true;
            OnIncreaseChanceOfSpawningEnemy?.Invoke(0.0001f);

            //If the PCPlayer has reached the final teleport pad, their progress is saved
            if(_isFinalPad)
            {
                OnCrossedCrookedBridges?.Invoke();
            }
        }

        //This saves if the PC Player has entered this teleport pad, and can also begin the teleport sequence immediately if required
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(true);

            if(_instantTeleport)
            {
                StartRingRotationRpc();
            }
        }
    }

    //This causes the rings of both the current and exit teleport pad to begin rotation, while playing the teleportation audio
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartRingRotationRpc()
    {
        if(!_teleportManager.arePadsReady) return;

        exitTeleportPad.GetComponent<TeleportPad>().rotateRings = true;
        rotateRings = true;

        _audioSource.Stop();
        _audioSource.clip = _teleportAudio;
        _audioSource.pitch = 3f;
        _audioSource.Play();
        _audioSource.enabled = true; 
    }

    //Stops the audio playing
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void StopAudioRpc()
    {
        _audioSource.Stop();
    }

    //Once ready, this teleports the PC Player to the exit pad, in the same position and rotation relative to the current teleport pad
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void TeleportRpc()
    {
        if(!_teleportManager.arePadsReady) return;
        
        _teleportManager.arePadsReady = false;
        StartCoroutine(TeleportPause());

        if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;

        Vector3 localPos = this.transform.InverseTransformPoint(_pcPlayerTransform.position);
        Quaternion localRot = Quaternion.Inverse(this.transform.rotation) * _pcPlayerTransform.GetChild(0).rotation;

        _pcPlayerTransform.position = exitTeleportPad.TransformPoint(localPos);
        _pcPlayerTransform.GetChild(0).rotation = exitTeleportPad.rotation * localRot;
    }

    private void FixedUpdate()
    {
        if(!rotateRings) return;
        
        //This rotates the rings
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || ring.rotation != restRotation) ring.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        //This rotates the rings in the opposite direction
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || reverseRing.rotation != restRotation) reverseRing.Rotate(Vector3.up * -rotateSpeed * Time.deltaTime);

        //This increases/decreases the rotation speed and teleportation visual effect (visuals only applied to the PC Player)
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed)
        {
            rotateSpeed += rotateIncrement;
            OnChangeTeleportRotateSpeed?.Invoke(rotateSpeed - minRotateSpeed, maxRotateSpeed - minRotateSpeed);
        }

        //Inverts the rotation increment to begin reducing rotation speed, and teleports the PC Player
        if(rotateSpeed >= maxRotateSpeed)
        {
            rotateIncrement *= -1f;   
            if(_isPlayerOnPad.Value) TeleportRpc();     
        }

        //If the rotation has slowed down enough, and the rings are at a rough angle, the rotation is stopped
        if( rotateIncrement < 0 && 
            rotateSpeed <= minRotateSpeed && 
            Quaternion.Angle(ring.rotation, restRotation) < rotationRange && 
            Quaternion.Angle(reverseRing.rotation, restRotation) < rotationRange)
        {
            rotateIncrement *= -1f;  
            rotateRings = false;

            ring.rotation = restRotation;
            reverseRing.rotation = restRotation;

            OnChangeTeleportRotateSpeed?.Invoke(0, maxRotateSpeed);
            
            StopAudioRpc();
        }
    }

    //Records if the PC Player has left the teleport pad
    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(false);
        }
    }

    //This stops the teleport pad from working for a period of time, to avoid multiple teleports at once
    IEnumerator TeleportPause()
    {
        yield return new WaitForSeconds(5f);

        if(rotateRings)
        {
            StartCoroutine(TeleportPause());
        }
        else
        {
            _teleportManager.arePadsReady = true;
        }
    }
}
