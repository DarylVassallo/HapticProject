using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    [SerializeField] private Renderer[] collectableIndicators;
    private int collectablePoints = 0;

    [SerializeField] private Material collectedMaterial;

    private void OnEnable()
    {
        HiddenTeleportButtonInteract.OnTriggerHiddenButton += GainHiddenButton;
    }

    private void OnDisable()
    {
        HiddenTeleportButtonInteract.OnTriggerHiddenButton -= GainHiddenButton;
    }

    private void GainHiddenButton()
    {
        collectableIndicators[collectablePoints].material.SetColor("_BaseColor", collectedMaterial.GetColor("_BaseColor"));
        collectablePoints++;
    }
}
