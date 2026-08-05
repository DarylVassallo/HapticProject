using UnityEngine;
using System;
//This is a basic script that triggers a win if the player reaches the centre of the Sauron level
public class WinPlatform : MonoBehaviour
{
    public static event Action OnWinGame;
    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject.CompareTag("PCPlayer") || other.gameObject.CompareTag("VRPlayer"))
        {
            OnWinGame?.Invoke();
        }
    }
}
