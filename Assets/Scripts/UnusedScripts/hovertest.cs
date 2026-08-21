using UnityEngine;

public class hovertest : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        socket.hoverEntered.AddListener(args =>
        {
            Debug.Log("Hover entered: " + args.interactableObject.transform.name);
            Debug.Log("socket.showInteractableHoverMeshes: " + socket.showInteractableHoverMeshes);
        });

        socket.hoverExited.AddListener(args =>
        {
            Debug.Log("Hover exited");
            Debug.Log("socket.showInteractableHoverMeshes: " + socket.showInteractableHoverMeshes);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
