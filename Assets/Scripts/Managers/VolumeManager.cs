using UnityEngine;

using UnityEngine.Rendering;

public class VolumeManager : MonoBehaviour
{
    private Volume healthDamageVolume;
    private Volume teleportVolume;

    void Awake()
    {
        Volume[] volumes = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponents<Volume>();

        foreach(var vol in volumes)
        {
            vol.weight = 0f;

            if (vol.priority == 0) teleportVolume = vol;
            if (vol.priority == 1) healthDamageVolume = vol;
        }
    }

    private void OnEnable()
    {
        EventsManager.OnChangeHealthCamera += ChangeHealthDamageVolume;
        EventsManager.OnChangeTeleportRotateSpeed += ChangeTeleportVolume;
    }

    private void OnDisable()
    {
        EventsManager.OnChangeHealthCamera -= ChangeHealthDamageVolume;
        EventsManager.OnChangeTeleportRotateSpeed -= ChangeTeleportVolume;
    }

    //This applies a damaged health visual effect to the PC Player, depending on the amount of health left (the less the PC Player's health is, the greater the visual intensity)
    private void ChangeHealthDamageVolume(float _currentHealth)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive) healthDamageVolume.weight = Mathf.Lerp(0f, 1f, 1f - _currentHealth / 100f);
    }

    //This applies a teleport visual effect to the PC Player, depending on the current rotational speed of the teleport rings (the greater the speed, the greater the visual intensity)
    private void ChangeTeleportVolume(float _currentRotateSpeed, float _maxRotateSpeed)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive) teleportVolume.weight = Mathf.Lerp(0f, 1f, _currentRotateSpeed / _maxRotateSpeed);
    }
}
