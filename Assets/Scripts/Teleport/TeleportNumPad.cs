using UnityEngine;
using TMPro;
using System;

using System.Collections;
using UnityEngine.XR.Content.Interaction;

public class TeleportNumPad : MonoBehaviour
{
    [Header("Symbols")]
    [SerializeField] private Material[] potentialSymbols;
    [SerializeField] private Material clearSymbol;
    [SerializeField] private Transform symbolsDisplay;
    private Renderer[] inputtedSymbols;
    private String inputtedCode;
    private int currentSymbolIndex;
    private bool _canAddNumber;

    [Header("Levers")]
    [SerializeField] private XRLever[] xrLevers;
    private bool[] activeLevers;

    [Header("Rope")]
    [SerializeField] private Transform ropeHandle;
    [SerializeField] private Transform ropeHandleEndPoint;
    private Rigidbody rbRopeHandle;
    private bool pullRope;
    private Vector3 originalRopeHandlePosition;

    void Awake()
    {
        currentSymbolIndex = 0;
        inputtedSymbols = new Renderer[symbolsDisplay.childCount];
        for (int i = 0; i < symbolsDisplay.childCount; i++)
        {
            inputtedSymbols[i] = symbolsDisplay.GetChild(i).GetComponent<Renderer>();
            inputtedSymbols[i].material = clearSymbol;
        }
        inputtedCode = "";
        _canAddNumber = true;



        activeLevers = new bool[5];
        for(int i = 0; i < activeLevers.Length; i++)
        {
            activeLevers[i] = false;
        }



        rbRopeHandle = ropeHandle.GetComponent<Rigidbody>();
        pullRope = true;
        originalRopeHandlePosition = ropeHandle.position;
    }

    private void OnEnable()
    {
        EventsManager.OnAddSymbolNumber += AddSymbol;
        EventsManager.OnClearTeleportNumPad += ClearInputtedSymbols;

        EventsManager.OnRemoveSymbol += RemoveLastSymbol;
        EventsManager.OnInputSymbols += InputCode;
    }

    private void OnDisable()
    {
        EventsManager.OnAddSymbolNumber -= AddSymbol;
        EventsManager.OnClearTeleportNumPad -= ClearInputtedSymbols;

        EventsManager.OnRemoveSymbol -= RemoveLastSymbol;
        EventsManager.OnInputSymbols -= InputCode;
    }

    //Adds the inputted symbol to the current visible inputted symbols
    private void AddSymbol(int _newNumber)
    {
        if((currentSymbolIndex >= inputtedSymbols.Length) || !_canAddNumber) return;

        _canAddNumber = false;

        inputtedSymbols[currentSymbolIndex].material = potentialSymbols[_newNumber];
        inputtedCode = inputtedCode + "" + (_newNumber + 1);
        
        currentSymbolIndex++;

        StartCoroutine(DelayAddNumber());
    }

    IEnumerator DelayAddNumber()
    {
        yield return new WaitForSeconds(0.25f);
        _canAddNumber = true;
    }

    //Removes the last inputted symbol
    private void RemoveLastSymbol()
    {
        if(currentSymbolIndex <= 0) return;
        currentSymbolIndex--;
        
        inputtedSymbols[currentSymbolIndex].material = clearSymbol;
        inputtedCode = inputtedCode.Substring(0, inputtedCode.Length - 1);
    }

    private void ClearInputtedSymbols()
    {
        for (int i = 0; i < inputtedSymbols.Length; i++)
        {
            inputtedSymbols[i].material = clearSymbol;
        }

        inputtedCode = "";
        currentSymbolIndex = 0;

        for (int i = 0; i < xrLevers.Length; i++)
        {
            xrLevers[i].value = false;
        }
    }

    //Sends the current code to be compared to existing correct codes.
    // It also resets the current code
    private void InputCode()
    {
        int _inputtedIntCode = -1;
        if(inputtedCode != "") _inputtedIntCode = int.Parse(inputtedCode);
        EventsManager.SendCodeToTeleportPads(_inputtedIntCode);
        ClearInputtedSymbols();
    }



    public void ActivateLever(int _leverIndex)
    {
        activeLevers[_leverIndex] = true;
        EventsManager.ActivateLever(_leverIndex);
    }

    public void DeactivateLever(int _leverIndex)
    {
        activeLevers[_leverIndex] = false;
        EventsManager.DeactivateLever(_leverIndex);
    }

    private void FixedUpdate()
    {
        if(!pullRope) return;

        if(ropeHandle.position.x >= originalRopeHandlePosition.x)
        {
            pullRope = false;
            rbRopeHandle.constraints = RigidbodyConstraints.FreezePosition;
        }
        
        rbRopeHandle.linearVelocity = new Vector3(2, 0, 0);
    }

    public void GrabbedRope()
    {
        rbRopeHandle.constraints = RigidbodyConstraints.None;
        pullRope = false;
    }
    
    public void ReleasedRope()
    {
        pullRope = true;        
    }
}
