using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Unity.Netcode;

//This script controls the flashlight's charge, as it runs out, the strength and range is also reduced. It can be charged, but the flash light is off during it.
public class FlashlightCharge : NetworkBehaviour
{
    private Light _spotLight;

    private NetworkVariable<float> _charge = new (100f);
    private NetworkVariable<bool> _isCharging = new (false);

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

    private bool _isNetworkSpawned = false;

    private void Awake()
    {
        _spotLight = this.GetComponentInChildren<Light>();

        _maxSpotLightIntensity = _spotLight.intensity;
        _maxSpotLightRange = _spotLight.range;

        _maxChargeBarLength = chargeBar.localScale.z;
    }

    private void OnEnable()
    {
        MazeManager.EnableDrainFlashlight += EnableDrainFlashlight;
        MazeManager.DisableDrainFlashlight += DisableDrainFlashlight;

        Debug.Log(this.gameObject + ": canUseChargeStation: " + canUseChargeStation);
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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _isNetworkSpawned = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDecayMultiplayerServerRpc(float _newDecay)
    {
        _decayMultiplier.Value = _newDecay;
    }

    private void EnableDrainFlashlight()
    {
        if(_isNetworkSpawned) SetDecayMultiplayerServerRpc(10f);
    }

    private void DisableDrainFlashlight()
    {
        if(_isNetworkSpawned) SetDecayMultiplayerServerRpc(1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIsChargingServerRpc(bool _newIsCharging)
    {
        Debug.Log(this.gameObject + " : New Change : " + _newIsCharging);
        _isCharging.Value = _newIsCharging;
        Debug.Log(this.gameObject + " : Change Charging To : " + _isCharging.Value);
    }

    private void ChargeFlashlightWithMouse(InputAction.CallbackContext context)
    {
        if(_isNetworkSpawned) SetIsChargingServerRpc(context.performed);
        ChangeSpotLightStrength(0, _isCharging.Value ? chargeIntensity : 1f);
    }

    private void ChargeFlashlightWithStation(bool charge)
    {
        Debug.Log(this.gameObject + ": ChargeFlashlightWithStation : " + charge);
        if(_isNetworkSpawned) SetIsChargingServerRpc(charge);
        Debug.Log(this.gameObject + ": 1 _isCharging.Value: " + _isCharging.Value);
        ChangeSpotLightStrength(0, _isCharging.Value ? chargeIntensity : 1f);
        Debug.Log(this.gameObject + ": 2 _isCharging.Value: " + _isCharging.Value);
    }

    private void ToggleFlashlight()
    {
        _spotLight.enabled = !_spotLight.enabled;
    }

    private void FixedUpdate()
    {      
        if ((_charge.Value >= 100 && _isCharging.Value) || (_charge.Value <= 0 && !_isCharging.Value && _spotLight.enabled)) return;

        if (_isCharging.Value && _charge.Value < 100)
        {
            _spotLight.enabled = true;
            ChangeSpotLightStrength(chargeRate / _decayMultiplier.Value, _isCharging.Value ? chargeIntensity : 1f);
            if(flashlightLever != null) flashlightLever.Rotate(Vector3.forward * Time.deltaTime * flashLightRotationSpeed);
        }else{
            ChangeSpotLightStrength(-decayRate * _decayMultiplier.Value, _isCharging.Value ? chargeIntensity : 1f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetChargeServerRpc(float _newCharge)
    {
        _charge.Value = _newCharge;
    }

    private void ChangeSpotLightStrength(float _change, float _brightness)
    {
        if(_isNetworkSpawned) SetChargeServerRpc(_charge.Value + _change);

        if(_charge.Value < 0)
        {
            if(_isNetworkSpawned) SetChargeServerRpc(0f);
        }else if(_charge.Value > 100)
        { 
            if(_isNetworkSpawned) SetChargeServerRpc(100f);
        }

        _spotLight.intensity = _maxSpotLightIntensity * (_charge.Value / 100f) * _brightness;
        _spotLight.range = _maxSpotLightRange * (_charge.Value / 100f) * _brightness;

        chargeBar.localScale = new Vector3  (   chargeBar.localScale.x, 
                                                chargeBar.localScale.y,
                                                _maxChargeBarLength * (_charge.Value / 100f)
                                            );

        if (this.CompareTag("PCFlashLight")) OnChangeChargeBar?.Invoke(_charge.Value);
    }
}
