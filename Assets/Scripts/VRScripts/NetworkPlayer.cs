using UnityEngine;
using Unity.Netcode;

//This script keeps the head and hands of the vr player to match the real player in a multiplayer network
//Source: https://www.youtube.com/watch?v=6fZ7LT5AeTw
public class NetworkPlayer : NetworkBehaviour
{
    public Transform root;
    public Transform head;
    public Transform chest;
    public Transform pelvis;
    public Transform leftHand;
    public Transform rightHand;

    public Renderer[] meshToDisable;

    public Quaternion headOffsetRotation;
    public Vector3 headOffsetPosition;

    public Quaternion chestOffsetRotation;
    public Vector3 chestOffsetPosition;

    public Quaternion pelvisOffsetRotation;
    public Vector3 pelvisOffsetPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            foreach (var item in meshToDisable)
            {
                item.enabled = false;
            }
        }
    }
    
    void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position + headOffsetPosition;
            head.rotation = Quaternion.Euler(   VRRigReferences.Singleton.head.rotation.eulerAngles + headOffsetRotation.eulerAngles);

            chest.position = new Vector3(   VRRigReferences.Singleton.head.position.x +  + chestOffsetPosition.x, 
                                            VRRigReferences.Singleton.head.position.y +  + chestOffsetPosition.y, 
                                            VRRigReferences.Singleton.head.position.z + chestOffsetPosition.z);
            chest.rotation = Quaternion.Euler(   chest.rotation.x + chestOffsetRotation.eulerAngles.x, 
                                                 VRRigReferences.Singleton.head.rotation.eulerAngles.y + chestOffsetRotation.eulerAngles.y, 
                                                 chest.rotation.z + chestOffsetRotation.eulerAngles.z);

            pelvis.position = new Vector3(   VRRigReferences.Singleton.head.position.x +  + pelvisOffsetPosition.x, 
                                            VRRigReferences.Singleton.head.position.y +  + pelvisOffsetPosition.y, 
                                            VRRigReferences.Singleton.head.position.z + pelvisOffsetPosition.z);
            pelvis.rotation = Quaternion.Euler(   pelvis.rotation.x + pelvisOffsetRotation.eulerAngles.x, 
                                                 VRRigReferences.Singleton.head.rotation.eulerAngles.y + pelvisOffsetRotation.eulerAngles.y, 
                                                 pelvis.rotation.z + pelvisOffsetRotation.eulerAngles.z);

            leftHand.position = VRRigReferences.Singleton.leftHand.position;
            leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

            rightHand.position = VRRigReferences.Singleton.rightHand.position;
            rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
        }
    }
}
