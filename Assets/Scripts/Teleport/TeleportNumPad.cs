using UnityEngine;
using TMPro;
using System;

using System.Collections;

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
    private bool[] activeLevers;

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
    }

    //Sends the current code to be compared to existing correct codes.
    // It also resets the current code
    private void InputCode()
    {
        int _inputtedIntCode = int.Parse(inputtedCode);
        if(inputtedCode != "") EventsManager.SendCodeToTeleportPads(_inputtedIntCode);
        ClearInputtedSymbols();
    }



    public void ActivateLever(int _leverIndex)
    {
        activeLevers[_leverIndex] = true;

        for(int i = 0; i < activeLevers.Length; i++)
        {
            Debug.Log("activate activeLevers[" + i + "]: " + activeLevers[i]);
        }
    }

    public void DeactivateLever(int _leverIndex)
    {
        activeLevers[_leverIndex] = false;

        for(int i = 0; i < activeLevers.Length; i++)
        {
            Debug.Log("deactivate activeLevers[" + i + "]: " + activeLevers[i]);
        }
    }
}
