using UnityEngine;

public class TrackVRSpotlight : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;

    void FixedUpdate()
    {
        Debug.Log($"World Pos: {targetPoint.position}");
        Debug.Log($"Local Pos: {targetPoint.localPosition}");
        Debug.Log($"Parent: {targetPoint.parent}");
        Debug.Log("Old Position: " + this.transform.position);
        Debug.Log(targetPoint + " : Target Position: " + targetPoint.position);
        this.transform.position = targetPoint.position;
        this.transform.rotation = targetPoint.rotation;
        Debug.Log("New Position: " + this.transform.position);
        Debug.Log("==========================");
    }
}
