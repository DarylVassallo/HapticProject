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

    private bool _isPlayerOnPad = false;
    private Transform _pcPlayerTransform;

    private TeleportManager _teleportManager;

    [SerializeField] private Transform bar;

    public static event Action<Transform> OnAddNewBar;
    // public static event Action<bool> OnSetPadsReady;
    public static event Action<float> OnIncreaseChanceOfSpawningEnemy;

    public bool arePadsReady = true;

    private bool hasBeenUsed = false;

    private bool instantTeleport;

    void Awake()
    {
        _codeText = this.GetComponentInChildren<TMP_Text>();
        _codeText.text = "";

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();
    }

    void Start()
    {
        OnAddNewBar?.Invoke(bar);
    }

    public override void OnNetworkSpawn()
    {
        if(exitTeleportPad != null)
        {
            Debug.Log("Start exitTeleportPad.position: " + exitTeleportPad.position);
            ExitPadServerRpc(exitTeleportPad.position);
        }
    }

    private void OnEnable()
    {
        TeleportNumPad.OnSendCode += CheckInputtedCode;
        // OnSetPadsReady += SetPadsReady;

        TeleportManager.OnEveythingCollected += ActivateInstantTeleport;
    }

    private void OnDisable()
    {
        TeleportNumPad.OnSendCode -= CheckInputtedCode;
        // OnSetPadsReady -= SetPadsReady;

        TeleportManager.OnEveythingCollected -= ActivateInstantTeleport;
    }

    private void ActivateInstantTeleport()
    {
        bar.parent.gameObject.SetActive(false);
        instantTeleport = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExitPadServerRpc(Vector3 newExitPad)
    {
        Debug.Log("ExitPadServerRpc: " + newExitPad);
        exitPad.Value = newExitPad;
        _secretCode.Value = UnityEngine.Random.Range(0, numLimit);
        _codeText.text = "" + _secretCode.Value + "";
    }

    private void CheckInputtedCode(int inputtedCode)
    {
        if(inputtedCode == _secretCode.Value && _isPlayerOnPad)
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
        _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenUsed)
        {
            hasBeenUsed = true;
            OnIncreaseChanceOfSpawningEnemy?.Invoke(0.001f);
        }

        if (other.CompareTag("PCPlayer") && _teleportManager.arePadsReady && instantTeleport)
        {
            _isPlayerOnPad = true;
            _pcPlayerTransform = other.transform;

            _teleportManager.arePadsReady = false;
            // OnSetPadsReady?.Invoke(false);
            StartCoroutine(TeleportPause());
            _positionDifference = exitPad.Value - this.transform.position;
            _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PCPlayer"))
        {
            _isPlayerOnPad = false;
            _pcPlayerTransform = null;
        }
    }

    IEnumerator TeleportPause()
    {
        yield return new WaitForSeconds(0.1f);
        _teleportManager.arePadsReady = true;
        // OnSetPadsReady?.Invoke(true);
    }
}
