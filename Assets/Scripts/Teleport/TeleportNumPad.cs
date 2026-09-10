using UnityEngine;
using TMPro;
using System;

using System.Collections;
using UnityEngine.XR.Content.Interaction;

using Unity.Netcode;

using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportNumPad : NetworkBehaviour
{
    private bool isInNetwork;

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
    private int pullingController;
    private Vector3 originalRopeHandlePosition;
    private float minDistance;
    private float maxDistance;
    private float prevDistance;
    private float currDistance;

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
        rbRopeHandle.constraints = RigidbodyConstraints.FreezePosition;

        pullingController = -1;

        originalRopeHandlePosition = ropeHandle.position;

        maxDistance = Vector3.Distance(ropeHandle.position, ropeHandleEndPoint.position);
        prevDistance = maxDistance;        
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

    public override void OnNetworkSpawn()
    {
        isInNetwork = true;
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
        Debug.Log("Activate lever");
        activeLevers[_leverIndex] = true;
        EventsManager.ActivateLever(_leverIndex);

        if(pullingController == 0)
        {
            EventsManager.PingVRController(0);
        }else if(pullingController == 1)
        {
            EventsManager.PingVRController(1);
        }
    }

    public void DeactivateLever(int _leverIndex)
    {
        Debug.Log("Deactivate lever");
        activeLevers[_leverIndex] = false;
        EventsManager.DeactivateLever(_leverIndex);

        if(pullingController == 0)
        {
            EventsManager.PingVRController(0);
        }else if(pullingController == 1)
        {
            EventsManager.PingVRController(1);
        }
    }

    public void GrabbedLever(SelectEnterEventArgs args)
    {
        Debug.Log("grabbed lever using: " + args.interactorObject.transform.name);

        if (args.interactorObject.transform.CompareTag("LeftHandInteractor"))
        {
            pullingController = 0;
        }else if (args.interactorObject.transform.CompareTag("RightHandInteractor"))
        {
            pullingController = 1;
        }
    }
    
    public void ReleasedLever(SelectExitEventArgs args)
    {
        Debug.Log("released lever using: " + args.interactorObject.transform.name); 

        if (args.interactorObject.transform.CompareTag("LeftHandInteractor"))
        {
            pullingController = 0;
        }else if (args.interactorObject.transform.CompareTag("RightHandInteractor"))
        {
            pullingController = 1;
        }    
    }

    private void FixedUpdate()
    {
        if(!isInNetwork) return;
        
        currDistance = Vector3.Distance(ropeHandle.position, ropeHandleEndPoint.position);
        if(prevDistance != currDistance)
        {
            EventsManager.ChangeHidingBarPosition(currDistance / maxDistance);
        }
        prevDistance = currDistance;

        if(!pullRope)
        {
            EventsManager.UseRopeHaptic(pullingController, 1f - (currDistance / maxDistance));
        }else{
            if(Vector3.Distance(ropeHandle.position, originalRopeHandlePosition) <= 0.5f)
            {
                pullRope = false;
                rbRopeHandle.constraints = RigidbodyConstraints.FreezePosition;
            }
            
            rbRopeHandle.linearVelocity = new Vector3(0, 20f * (1f - (currDistance / maxDistance)), 20f * (1f - (currDistance / maxDistance)));
        }
    }

    public void GrabbedRope(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.CompareTag("LeftHandInteractor"))
        {
            pullingController = 0;
        }else if (args.interactorObject.transform.CompareTag("RightHandInteractor"))
        {
            pullingController = 1;
        }

        pullRope = false;
        rbRopeHandle.constraints = RigidbodyConstraints.None;
    }
    
    public void ReleasedRope(SelectExitEventArgs args)
    {
        pullRope = true;  
        pullingController = -1;      
    }
}
