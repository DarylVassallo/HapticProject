using UnityEngine;
using TMPro;

using Unity.Netcode;
using System;
using System.Collections;

public class TeleportPad : NetworkBehaviour
{
    [SerializeField] private Transform exitTeleportPad;
    public NetworkVariable<Vector3> exitPad = new (new Vector3(0, 0, 0));

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

    public bool arePadsReady = true;

    private bool _hasBeenUsed = false;

    private bool _instantTeleport;

    void Awake()
    {
        _codeText = this.GetComponentInChildren<TMP_Text>();
        _codeText.text = "";

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();

        _instantTeleport = true;
    }

    void Start()
    {
        OnAddNewBar?.Invoke(bar);
    }

    private void OnEnable()
    {
        TeleportNumPad.OnSendCode += CheckInputtedCode;
        // OnSetPadsReady += SetPadsReady;
        TeleportManager.OnEveythingCollected += ActivateInstantTeleport;
        CheckpointManager.OnResetTeleportPads += ResetTeleportPad;

        _secretCode.OnValueChanged += ChangeCodeText;
    }

    private void OnDisable()
    {
        TeleportNumPad.OnSendCode -= CheckInputtedCode;
        // OnSetPadsReady -= SetPadsReady;
        TeleportManager.OnEveythingCollected -= ActivateInstantTeleport;
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

    [ServerRpc(RequireOwnership = false)]
    public void ExitPadServerRpc(Vector3 _newExitPad)
    {        
        exitPad.Value = _newExitPad;
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
        _teleportManager.arePadsReady = false;
        // OnSetPadsReady?.Invoke(false);
        _positionDifference = exitPad.Value - this.transform.position;

        if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Transform>();
        _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (!_hasBeenUsed)
        {
            _hasBeenUsed = true;
            OnIncreaseChanceOfSpawningEnemy?.Invoke(0.0002f);
        }

        if (_other.CompareTag("PCPlayer") && _teleportManager.arePadsReady)
        {
            ChangeIsPlayerOnPadServerRpc(true);
            // _pcPlayerTransform = _other.transform;

            if(_instantTeleport)
            {
                _teleportManager.arePadsReady = false;
                // OnSetPadsReady?.Invoke(false);
                StartCoroutine(TeleportPause());
                _positionDifference = exitPad.Value - this.transform.position;

                if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Transform>();
                _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
            }
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
        yield return new WaitForSeconds(0.1f);
        _teleportManager.arePadsReady = true;
        // OnSetPadsReady?.Invoke(true);
    }
}
