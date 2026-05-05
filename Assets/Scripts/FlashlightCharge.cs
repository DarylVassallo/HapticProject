using UnityEngine;
using UnityEngine.InputSystem;
using System;
//This script controls the flashlight's charge, as it runs out, the strength and range is also reduced. It can be charged, but the flash light is off during it.
public class FlashlightCharge : MonoBehaviour
{
    private Light _spotLight;

    private float _charge;
    private bool _isCharging;

    [SerializeField] private float decayRate;
    [SerializeField] private float chargeRate;
    [SerializeField] private float flashLightRotationSpeed;

    private float _maxSpotLightIntensity;
    private float _maxSpotLightRange;

    [SerializeField] private float chargeIntensity;

    [SerializeField] private Transform flashlightLever;

    private int _suddenDrain;

    private void Awake()
    {
        _spotLight = this.GetComponentInChildren<Light>();
        _charge = 100f;

        _maxSpotLightIntensity = _spotLight.intensity;
        _maxSpotLightRange = _spotLight.range;

        _suddenDrain = 0;
    }

    private void OnEnable()
    {
        PCPlayerInputManager.OnFire += ChargeFlashlight;
        PCPlayerInputManager.OnFire2 += ToggleFlashlight;

        MazeManager.OnDrainFlashlight += DrainFlashlight;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnFire -= ChargeFlashlight;
        PCPlayerInputManager.OnFire2 -= ToggleFlashlight;

        MazeManager.OnDrainFlashlight -= DrainFlashlight;
    }

    private void DrainFlashlight()
    {
        _suddenDrain = 100;
        // ChangeSpotLightStrength(-decayRate * 999999, _isCharging ? chargeIntensity : 1f);
    }

    private void ChargeFlashlight(InputAction.CallbackContext context)
    {
        _isCharging = context.performed;

        ChangeSpotLightStrength(0, _isCharging ? chargeIntensity : 1f);
    }

    private void ToggleFlashlight()
    {
        _spotLight.enabled = !_spotLight.enabled;
    }

    private void FixedUpdate()
    {      
        if (_suddenDrain > 0)
        {
            ChangeSpotLightStrength(-decayRate * 100, _isCharging ? chargeIntensity : 1f);
            _suddenDrain--;
        }else{
            if ((_charge >= 100 && _isCharging) || (_charge <= 0 && !_isCharging && _spotLight.enabled)) return;

            if (_isCharging && _charge < 100)
            {
                _spotLight.enabled = true;
                ChangeSpotLightStrength(chargeRate, _isCharging ? chargeIntensity : 1f);
                flashlightLever.Rotate(Vector3.forward * Time.deltaTime * flashLightRotationSpeed);
            }else{
                ChangeSpotLightStrength(-decayRate, _isCharging ? chargeIntensity : 1f);
            }
        }
    }

    private void ChangeSpotLightStrength(float _change, float _brightness)
    {
        _charge += _change;
        if(_charge < 0) _charge = 0;
        if(_charge > 100) _charge = 100;

        _spotLight.intensity = _maxSpotLightIntensity * (_charge / 100f) * _brightness;
        _spotLight.range = _maxSpotLightRange * (_charge / 100f) * _brightness;
    }
}
