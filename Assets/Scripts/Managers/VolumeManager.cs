using UnityEngine;

using UnityEngine.Rendering;

public class VolumeManager : MonoBehaviour
{
    private Volume healthDamageVolume;

    private Volume _teleportVolume;
    private bool _canTeleportVolumeChange;

    void Awake()
    {
        _canTeleportVolumeChange = true;

        Volume[] volumes = GameObject.FindGameObjectWithTag("GlobalVolume").GetComponents<Volume>();

        foreach(var vol in volumes)
        {
            vol.weight = 0f;

            if (vol.priority == 0) _teleportVolume = vol;
            if (vol.priority == 1) healthDamageVolume = vol;
        }
    }

    private void OnEnable()
    {
        EventsManager.OnChangeHealthCamera += ChangeHealthDamageVolume;
        EventsManager.OnResetHealth += ResetHealthDamageVolume;
        EventsManager.OnChangeTeleportRotateSpeed += ChangeTeleportVolume;
        EventsManager.OnFixTeleportEffect += FixTeleportEffect;
    }

    private void OnDisable()
    {
        EventsManager.OnChangeHealthCamera -= ChangeHealthDamageVolume;
        EventsManager.OnResetHealth -= ResetHealthDamageVolume;
        EventsManager.OnChangeTeleportRotateSpeed -= ChangeTeleportVolume;
        EventsManager.OnFixTeleportEffect -= FixTeleportEffect;
    }

    //This applies a damaged health visual effect to the PC Player, depending on the amount of health left (the less the PC Player's health is, the greater the visual intensity)
    private void ChangeHealthDamageVolume(float _currentHealth)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive) healthDamageVolume.weight = Mathf.Lerp(0f, 1f, 1f - _currentHealth / 100f);
    }

    private void ResetHealthDamageVolume(GameObject _player)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive) healthDamageVolume.weight = 0f;
    }

    //This applies a teleport visual effect to the PC Player, depending on the current rotational speed of the teleport rings (the greater the speed, the greater the visual intensity)
    private void ChangeTeleportVolume(float _currentRotateSpeed, float _maxRotateSpeed)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive && _canTeleportVolumeChange)
        {
            _teleportVolume.weight = Mathf.Lerp(0f, 1f, _currentRotateSpeed / _maxRotateSpeed);
        }
    }

    private void FixTeleportEffect(float _teleportEffect, bool _canChange)
    {
        if(!UnityEngine.XR.XRSettings.isDeviceActive)
        {
            _teleportVolume.weight = _teleportEffect;
            _canTeleportVolumeChange = _canChange;
        }
    }
}
