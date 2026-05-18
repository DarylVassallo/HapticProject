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
    
    [SerializeField] private Transform chargeBar;
    private float _maxChargeBarLength;

    [SerializeField] private bool canUseChargeStation;

    public static event Action<float> OnChangeChargeBar;

    private void Awake()
    {
        _spotLight = this.GetComponentInChildren<Light>();
        _charge = 100f;

        _maxSpotLightIntensity = _spotLight.intensity;
        _maxSpotLightRange = _spotLight.range;

        _suddenDrain = 0;

        _maxChargeBarLength = chargeBar.localScale.z;
    }

    private void OnEnable()
    {
        MazeManager.OnDrainFlashlight += DrainFlashlight;

        if(canUseChargeStation)
        {
            ChargeStation.OnCharge += ChargeFlashlightWithStation;
        }
        else
        {
            PCPlayerInputManager.OnFire += ChargeFlashlightWithMouse;
            PCPlayerInputManager.OnFire2 += ToggleFlashlight;
        }
    }

    private void OnDisable()
    {
        MazeManager.OnDrainFlashlight -= DrainFlashlight;

        if(canUseChargeStation)
        {
            ChargeStation.OnCharge -= ChargeFlashlightWithStation;
        }
        else
        {
            PCPlayerInputManager.OnFire -= ChargeFlashlightWithMouse;
            PCPlayerInputManager.OnFire2 -= ToggleFlashlight;
        }
    }

    private void DrainFlashlight()
    {
        _suddenDrain = 100;
        // ChangeSpotLightStrength(-decayRate * 999999, _isCharging ? chargeIntensity : 1f);
    }

    private void ChargeFlashlightWithMouse(InputAction.CallbackContext context)
    {
        _isCharging = context.performed;
        ChangeSpotLightStrength(0, _isCharging ? chargeIntensity : 1f);
    }

    private void ChargeFlashlightWithStation(bool charge)
    {
        _isCharging = charge;
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

        chargeBar.localScale = new Vector3  (   chargeBar.localScale.x, 
                                                chargeBar.localScale.y,
                                                _maxChargeBarLength * (_charge / 100f)
                                            );

        if (this.CompareTag("PCFlashLight")) OnChangeChargeBar?.Invoke(_charge);
    }
}
