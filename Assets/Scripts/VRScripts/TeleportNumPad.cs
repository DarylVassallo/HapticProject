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
        OnSendCode?.Invoke(int.Parse(codeText.text));
        codeText.text = "";
    }
}
