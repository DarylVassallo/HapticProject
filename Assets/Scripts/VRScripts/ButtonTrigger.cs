using UnityEngine;


public class ButtonTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.AddListener(x => Trigger());
    }

    public void Trigger()
    {
        Debug.Log("TRIGGER");
    }
}
