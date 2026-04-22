using UnityEngine;
//This script stores the references for the VR Player's head and hands (STILL IN DEVELOPMENT).
//Source: https://www.youtube.com/watch?v=6fZ7LT5AeTw
public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Awake()
    {
        Singleton = this;
    }
}
