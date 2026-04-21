using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PCPlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    [SerializeField] private GameObject camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.red;
            camera.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                input.x -= 1;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                input.x += 1;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                input.y -= 1;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                input.y += 1;
        }

        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
