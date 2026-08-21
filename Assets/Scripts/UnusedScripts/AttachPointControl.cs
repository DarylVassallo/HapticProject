using UnityEngine;

public class AttachPointControl : MonoBehaviour
{
    public Transform hand;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position = hand.position;
        this.transform.rotation = hand.rotation;
    }
}
