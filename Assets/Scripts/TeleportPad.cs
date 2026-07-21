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
    // public static event Action<bool> OnSetPadsReady;
    public static event Action<float> OnIncreaseChanceOfSpawningEnemy;
    public static event Action OnCrossedCrookedBridges;

    public bool arePadsReady = true;

    private bool _hasBeenUsed = false;

    private bool _instantTeleport;

    public bool rotateRings = false;

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
        TeleportNumPad.OnSendCode += CheckInputtedCode;
        // OnSetPadsReady += SetPadsReady;
        TeleportManager.OnEverythingCollected += ActivateInstantTeleport;
        CheckpointManager.OnResetTeleportPads += ResetTeleportPad;

        _secretCode.OnValueChanged += ChangeCodeText;
    }

    private void OnDisable()
    {
        TeleportNumPad.OnSendCode -= CheckInputtedCode;
        // OnSetPadsReady -= SetPadsReady;
        TeleportManager.OnEverythingCollected -= ActivateInstantTeleport;
        CheckpointManager.OnResetTeleportPads -= ResetTeleportPad;

        _secretCode.OnValueChanged -= ChangeCodeText;
    }

    private void ResetTeleportPad()
    {
        _hasBeenUsed = false;
    }

    private void ActivateInstantTeleport()
    {
        bar.parent.gameObject.SetActive(false);
        _instantTeleport = true;
    }

    public void SetExitPadTransform(Transform _newExitPadTransform)
    {
        exitTeleportPad = _newExitPadTransform;
        ChangeSecretCodePadServerRpc();
    }

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

    private void ChangeCodeText(int _previous, int _current)
    {
        _codeText.text = "" + _current + "";
    }

    private void CheckInputtedCode(int _inputtedCode)
    {
        if(_inputtedCode == _secretCode.Value && _isPlayerOnPad.Value)
        {
            StartRingRotationRpc();
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (!_hasBeenUsed)
        {
            _hasBeenUsed = true;
            OnIncreaseChanceOfSpawningEnemy?.Invoke(0.0001f);

            if(_isFinalPad)
            {
                OnCrossedCrookedBridges?.Invoke();
            }
        }

        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(true);

            if(_instantTeleport)
            {
                StartRingRotationRpc();
            }
        }
    }

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

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void StopAudioRpc()
    {
        _audioSource.Stop();
    }

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
        
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || ring.rotation != restRotation)
        {
            ring.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || reverseRing.rotation != restRotation)
        {
            reverseRing.Rotate(Vector3.up * -rotateSpeed * Time.deltaTime);
        }

        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed)
        {
            rotateSpeed += rotateIncrement;
            Debug.Log(this.gameObject + " : Change Rotate Speed");
            OnChangeTeleportRotateSpeed?.Invoke(rotateSpeed - minRotateSpeed, maxRotateSpeed - minRotateSpeed);
        }

        if(rotateSpeed >= maxRotateSpeed)
        {
            rotateIncrement *= -1f;   
            if(_isPlayerOnPad.Value)
            {
                TeleportRpc();     
            }
        }

        if( rotateIncrement < 0 && 
            rotateSpeed <= minRotateSpeed && 
            Quaternion.Angle(ring.rotation, restRotation) < rotationRange && 
            Quaternion.Angle(reverseRing.rotation, restRotation) < rotationRange)
        {
            rotateIncrement *= -1f;  
            rotateRings = false;

            ring.rotation = restRotation;
            reverseRing.rotation = restRotation;

            Debug.Log(this.gameObject + " : Stop Rotating");
            OnChangeTeleportRotateSpeed?.Invoke(0, maxRotateSpeed);
            
            StopAudioRpc();
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(false);
        }
    }

    IEnumerator TeleportPause()
    {
        yield return new WaitForSeconds(1f);

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
