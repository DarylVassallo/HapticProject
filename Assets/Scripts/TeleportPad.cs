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

    void Awake()
    {
        _codeText = this.GetComponentInChildren<TMP_Text>();
        _codeText.text = "";

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();

        _instantTeleport = false;

        restRotation = ring.rotation;
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
            TeleportPCPlayerClientRpc();
        }
    }

    // private void SetPadsReady(bool _newArePadsReady)
    // {
    //     arePadsReady = _newArePadsReady;
    // }

    [ClientRpc]
    public void TeleportPCPlayerClientRpc()
    {
        Teleport();

        // _teleportManager.arePadsReady = false;
        // // OnSetPadsReady?.Invoke(false);
        // _positionDifference = exitTeleportPad.position - this.transform.position;

        // if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Transform>();
        // _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
    }

    private void OnTriggerEnter(Collider _other)
    {
        Debug.Log(this.gameObject + " : OnTriggerEnter");
        if (!_hasBeenUsed)
        {
            _hasBeenUsed = true;
            OnIncreaseChanceOfSpawningEnemy?.Invoke(0.0002f);

            if(_isFinalPad)
            {
                OnCrossedCrookedBridges?.Invoke();
            }
        }

        if (_other.CompareTag("PCPlayer") && _teleportManager.arePadsReady)
        {
            ChangeIsPlayerOnPadServerRpc(true);
            // _pcPlayerTransform = _other.transform;

            if(_instantTeleport)
            {
                exitTeleportPad.GetComponent<TeleportPad>().rotateRings = true;
                rotateRings = true;
                // Teleport();
            }
        }
    }

    private void Teleport()
    {
        if(!_teleportManager.arePadsReady) return;
        _teleportManager.arePadsReady = false;
        // OnSetPadsReady?.Invoke(false);
        StartCoroutine(TeleportPause());

        if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Transform>();

        Vector3 localPos = this.transform.InverseTransformPoint(_pcPlayerTransform.position);
        Quaternion localRot = Quaternion.Inverse(this.transform.rotation) * _pcPlayerTransform.rotation;

        // _positionDifference = exitTeleportPad.position - this.transform.position;

        _pcPlayerTransform.position = exitTeleportPad.TransformPoint(localPos);
        _pcPlayerTransform.rotation = exitTeleportPad.rotation * localRot;
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

        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed) rotateSpeed += rotateIncrement;

        if(rotateSpeed >= maxRotateSpeed)
        {
            rotateIncrement *= -1f;   
            if(_isPlayerOnPad.Value)
            {
                Teleport();     
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
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(false);
            // _pcPlayerTransform = null;
        }
    }

    IEnumerator TeleportPause()
    {
        yield return new WaitForSeconds(1f);
        _teleportManager.arePadsReady = true;
        // OnSetPadsReady?.Invoke(true);
    }
}
