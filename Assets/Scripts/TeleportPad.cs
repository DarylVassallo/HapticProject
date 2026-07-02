using UnityEngine;
using TMPro;

using Unity.Netcode;
using System;
using System.Collections;

public class TeleportPad : NetworkBehaviour
{
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

    void Awake()
    {
        _codeText = this.GetComponentInChildren<TMP_Text>();
        _codeText.text = "";

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();
    }

    void Start()
    {
        Debug.Log(this.gameObject + ": OnAddNewBar");
        OnAddNewBar?.Invoke(bar);
    }

    private void OnEnable()
    {
        TeleportNumPad.OnSendCode += CheckInputtedCode;
    }

    private void OnDisable()
    {
        TeleportNumPad.OnSendCode -= CheckInputtedCode;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExitPadServerRpc(Vector3 newExitPad)
    {
        exitPad.Value = newExitPad;
        _secretCode.Value = UnityEngine.Random.Range(0, numLimit);
        _codeText.text = "" + _secretCode.Value + "";

        Debug.Log(this.gameObject + ": exitPad.Value: " + exitPad.Value);
    }

    private void CheckInputtedCode(int inputtedCode)
    {
        Debug.Log("=======");
        Debug.Log("CheckInputtedCode");
        Debug.Log("inputtedCode: " + inputtedCode);
        Debug.Log("_secretCode.Value: " + _secretCode.Value);
        Debug.Log("_isPlayerOnPad: " + _isPlayerOnPad);

        if(inputtedCode == _secretCode.Value && _isPlayerOnPad)
        {
            Debug.Log("Teleport");
            TeleportPCPlayerClientRpc();
        }
    }

    [ClientRpc]
    public void TeleportPCPlayerClientRpc()
    {
        Debug.Log("--------");
        Debug.Log("TeleportPCPlayerClientRpc");

        _teleportManager.arePadsReady = false;
        _positionDifference = exitPad.Value - this.transform.position;
        Debug.Log("_positionDifference: " + _positionDifference);
        Debug.Log("old _pcPlayerTransform.position: " + _pcPlayerTransform.position);

        _pcPlayerTransform.position = _pcPlayerTransform.position + _positionDifference;
        Debug.Log("new _pcPlayerTransform.position: " + _pcPlayerTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PCPlayer") && _teleportManager.arePadsReady)
        {
            _isPlayerOnPad = true;
            _pcPlayerTransform = other.transform;

            _teleportManager.arePadsReady = false;
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
    }
}
