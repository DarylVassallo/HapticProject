using UnityEngine;
using TMPro;

using Unity.Netcode;


public class TeleportPad : NetworkBehaviour
{
    [SerializeField] private Transform exitPad;

    private Vector3 positionDifference;

    public bool isPadReady = true;

    private TMP_Text codeText;
    private NetworkVariable<int> secretCode = new (0);
    private NetworkVariable<bool> generatedSecretCode = new (false);
    [SerializeField] private int numLimit;

    private bool isPlayerOnPad = false;
    private Transform pcPlayerTransform;

    void Awake()
    {
        codeText = this.GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        TeleportNumPad.OnSendCode += CheckInputtedCode;
    }

    private void OnDisable()
    {
        TeleportNumPad.OnSendCode -= CheckInputtedCode;
    }

    public override void OnNetworkSpawn()
    {
        SecretCodeServerRpc(Random.Range(0, numLimit));
        codeText.text = "" + secretCode.Value + "";
    }

    [ServerRpc(RequireOwnership = false)]
    public void SecretCodeServerRpc(int newSecretCode)
    {
        if(!generatedSecretCode.Value)
        {
            secretCode.Value = newSecretCode;
            generatedSecretCode.Value = true;
        }
    }

    private void CheckInputtedCode(int inputtedCode)
    {
        Debug.Log("=======");
        Debug.Log("CheckInputtedCode");
        Debug.Log("inputtedCode: " + inputtedCode);
        Debug.Log("secretCode.Value: " + secretCode.Value);
        Debug.Log("isPlayerOnPad: " + isPlayerOnPad);

        if(inputtedCode == secretCode.Value && isPlayerOnPad)
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
        exitPad.GetComponent<TeleportPad>().isPadReady = false;
        positionDifference = exitPad.position - this.transform.position;
        Debug.Log("positionDifference: " + positionDifference);
        Debug.Log("old pcPlayerTransform.position: " + pcPlayerTransform.position);
        pcPlayerTransform.position = pcPlayerTransform.position + positionDifference;
        Debug.Log("new pcPlayerTransform.position: " + pcPlayerTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PCPlayer") && isPadReady)
        {
            isPlayerOnPad = true;
            pcPlayerTransform = other.transform;

            // exitPad.GetComponent<TeleportPad>().isPadReady = false;
            // positionDifference = exitPad.position - this.transform.position;
            // pcPlayerTransform.position = pcPlayerTransform.position + positionDifference;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PCPlayer"))
        {
            isPlayerOnPad = false;
            pcPlayerTransform = null;

            isPadReady = true;
        }
    }
}
