using UnityEngine;
using TMPro;
using System;

public class TeleportNumPad : MonoBehaviour
{
    private TMP_Text codeText;

    public static event Action<int> OnSendCode;

    void Awake()
    {
        codeText = this.GetComponentInChildren<TMP_Text>();
        codeText.text = "";
    }

    public void AddNumber(int _newNumber)
    {
        codeText.text = codeText.text + "" + _newNumber + "";
    }

    public void ResetCode()
    {
        codeText.text = "";
    }

    public void InputCode()
    {
        Debug.Log("InputCode");
        Debug.Log("codeText.text: " + codeText.text);
        // Debug.Log("int.Parse(codeText.text): " + int.Parse(codeText.text));
        if(codeText.text != "") OnSendCode?.Invoke(int.Parse(codeText.text));
        codeText.text = "";
    }
}
