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

    //Adds the inputted number to the current visible code
    public void AddNumber(int _newNumber)
    {
        codeText.text = codeText.text + "" + _newNumber + "";
    }

    //Resets the current visible code
    public void ResetCode()
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
