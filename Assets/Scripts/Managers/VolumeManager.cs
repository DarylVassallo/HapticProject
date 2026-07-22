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
        Health.OnChangeHealthCamera += ChangeHealthDamageVolume;
        TeleportPad.OnChangeTeleportRotateSpeed += ChangeTeleportVolume;
    }

    private void OnDisable()
    {
        Health.OnChangeHealthCamera -= ChangeHealthDamageVolume;
        TeleportPad.OnChangeTeleportRotateSpeed -= ChangeTeleportVolume;
    }

    private void ChangeHealthDamageVolume(float _currentHealth)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive) healthDamageVolume.weight = Mathf.Lerp(0f, 1f, 1f - _currentHealth / 100f);
    }

    private void ChangeTeleportVolume(float _currentRotateSpeed, float _maxRotateSpeed)
    {
        Debug.Log("_currentRotateSpeed: " + _currentRotateSpeed);
        Debug.Log("_currentRotateSpeed: " + _maxRotateSpeed);
        if(!UnityEngine.XR.XRSettings.isDeviceActive) teleportVolume.weight = Mathf.Lerp(0f, 1f, _currentRotateSpeed / _maxRotateSpeed);
    }
}
