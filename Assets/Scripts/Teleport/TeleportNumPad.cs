using UnityEngine;
using TMPro;
using System;

public class TeleportNumPad : MonoBehaviour
{
    private TMP_Text codeText;

    void Awake()
    {
        codeText = this.GetComponentInChildren<TMP_Text>();
        codeText.text = "";
    }

    private void OnEnable()
    {
        EventsManager.OnClearTeleportNumPad += ClearCode;
    }

    private void OnDisable()
    {
        EventsManager.OnClearTeleportNumPad -= ClearCode;
    }

    //Adds the inputted number to the current visible code
    public void AddNumber(int _newNumber)
    {
        codeText.text = codeText.text + "" + _newNumber + "";
    }

    //Resets the current visible code
    public void ResetCode()
    {
        if(codeText.text.Length > 0) codeText.text = codeText.text.Substring(0, codeText.text.Length - 1);
    }

    private void ClearCode()
    {
        codeText.text = "";
    }

    //Sends the current code to be compared to existing correct codes.
    // It also resets the current code
    public void InputCode()
    {
        if(codeText.text != "") EventsManager.SendCodeToTeleportPads(int.Parse(codeText.text));
        codeText.text = "";
    }
}
