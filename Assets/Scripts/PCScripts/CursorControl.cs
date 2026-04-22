using UnityEngine;
//This script hides the cursor for the PC Player.
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class CursorControl : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
