using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Unity.Netcode;

//This script controls the flashlight's charge, as it runs out, the strength and range is also reduced. It can be charged, but the flash light is off during it.
public class FlashlightCharge : NetworkBehaviour
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
    
    [SerializeField] private Transform chargeBar;
    private float _maxChargeBarLength;

    [SerializeField] private bool canUseChargeStation;

    public static event Action<float> OnChangeChargeBar;

    private NetworkVariable<float> _decayMultiplier = new (1f);

    private void Awake()
    {
        _spotLight = this.GetComponentInChildren<Light>();
        _charge = 100f;

        _maxSpotLightIntensity = _spotLight.intensity;
        _maxSpotLightRange = _spotLight.range;

        _maxChargeBarLength = chargeBar.localScale.z;
    }

    private void OnEnable()
    {
        MazeManager.EnableDrainFlashlight += EnableDrainFlashlight;
        MazeManager.DisableDrainFlashlight += DisableDrainFlashlight;

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
        MazeManager.EnableDrainFlashlight -= EnableDrainFlashlight;
        MazeManager.DisableDrainFlashlight -= DisableDrainFlashlight;

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

    [ServerRpc(RequireOwnership = false)]
    private void SetDecayMultiplayerServerRpc(float _newDecay)
    {
        _decayMultiplier.Value = _newDecay;
    }

    private void EnableDrainFlashlight()
    {
        SetDecayMultiplayerServerRpc(10f);
    }

    private void DisableDrainFlashlight()
    {
        SetDecayMultiplayerServerRpc(1f);
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
        if ((_charge >= 100 && _isCharging) || (_charge <= 0 && !_isCharging && _spotLight.enabled)) return;

        if (_isCharging && _charge < 100)
        {
            _spotLight.enabled = true;
            ChangeSpotLightStrength(chargeRate / _decayMultiplier.Value, _isCharging ? chargeIntensity : 1f);
            if(flashlightLever != null) flashlightLever.Rotate(Vector3.forward * Time.deltaTime * flashLightRotationSpeed);
        }else{
            ChangeSpotLightStrength(-decayRate * _decayMultiplier.Value, _isCharging ? chargeIntensity : 1f);
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
