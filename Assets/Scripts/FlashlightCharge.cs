using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

//This script controls the flashlight's charge, as it runs out, the strength and range is also reduced. It can be charged, but the flash light is off during it.
public class FlashlightCharge : NetworkBehaviour
{
    public Light spotLight;

    private NetworkVariable<bool> _spotLightToggle = new (true);

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

    private bool _shuttingDown = true;

    [SerializeField] private AudioClip handCrankAudio;
    [SerializeField] private AudioClip chargeStationAudio;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _maxSpotLightIntensity = spotLight.intensity;
        _maxSpotLightRange = spotLight.range;

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
            PCPlayerInputManager.OnFire2 += SetFlashlightEnableServerRpc;
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
            PCPlayerInputManager.OnFire2 -= SetFlashlightEnableServerRpc;
        }
    }

    public override void OnNetworkSpawn()
    {
        _isNetworkSpawned = true;
        _shuttingDown = false;
    }

    public override void OnNetworkDespawn()
    {
        _shuttingDown = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDecayMultiplayerServerRpc(float _newDecay)
    {
        _decayMultiplier.Value = _newDecay;
    }

    [ClientRpc]
    private void PlayAudioClientRpc(int _audioNum)
    {
        AudioClip _currentAudio = handCrankAudio;
        switch (_audioNum)
        {
            case 0:
                _currentAudio = handCrankAudio;
                break;
            case 1:
                _currentAudio = chargeStationAudio;
                break;
        }

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = _currentAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 
        }
    }

    [ClientRpc]
    private void StopAudioClientRpc()
    {
        _audioSource.Stop();
    }

    public void socketRelease(SelectExitEventArgs args)
    {
        Debug.Log("Release");
        var xr = args.interactableObject;

        xr.transform.SetParent(null, true);
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
        _isCharging.Value = _newIsCharging;
    }

    private void ChargeFlashlightWithMouse(InputAction.CallbackContext context)
    {
        if(_isNetworkSpawned) SetIsChargingServerRpc(context.performed);
        ChangeSpotLightStrength(0, _isCharging.Value ? chargeIntensity : 1f);
    }

    private void ChargeFlashlightWithStation(bool charge)
    {
        if(_isNetworkSpawned) SetIsChargingServerRpc(charge);
        ChangeSpotLightStrength(0, _isCharging.Value ? chargeIntensity : 1f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFlashlightEnableServerRpc()
    {
        _spotLightToggle.Value = !_spotLightToggle.Value;
    }

    private void FixedUpdate()
    {      
        if(spotLight.enabled != _spotLightToggle.Value)
        {
            spotLight.enabled = _spotLightToggle.Value;
        }
        
        if(_charge.Value >= 100 && _isCharging.Value && !_shuttingDown) StopAudioClientRpc();

        if ((_charge.Value >= 100 && _isCharging.Value) || (_charge.Value <= 0 && !_isCharging.Value && spotLight.enabled)) return;

        if (_isCharging.Value && _charge.Value < 100)
        {
            if(!_spotLightToggle.Value) SetFlashlightEnableServerRpc();
            ChangeSpotLightStrength(chargeRate / _decayMultiplier.Value, _isCharging.Value ? chargeIntensity : 1f);

            if(flashlightLever != null)
            {
                flashlightLever.Rotate(Vector3.forward * Time.deltaTime * flashLightRotationSpeed);
                if(!_shuttingDown) PlayAudioClientRpc(0);
            }else if(canUseChargeStation)
            {
                if(!_shuttingDown) PlayAudioClientRpc(1);
            }
        }else{
            ChangeSpotLightStrength(-decayRate * _decayMultiplier.Value, _isCharging.Value ? chargeIntensity : 1f);
            if(!_shuttingDown) StopAudioClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetChargeServerRpc(float _newCharge)
    {
        _charge.Value = _newCharge;
    }

    private void ChangeSpotLightStrength(float _change, float _brightness)
    {        
        if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(_charge.Value + _change);

        if(_charge.Value < 0)
        {
            if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(0f);
        }else if(_charge.Value > 100)
        { 
            if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(100f);
        }

        spotLight.intensity = _maxSpotLightIntensity * (_charge.Value / 100f) * _brightness;
        spotLight.range = _maxSpotLightRange * (_charge.Value / 100f) * _brightness;

        chargeBar.localScale = new Vector3  (   chargeBar.localScale.x, 
                                                chargeBar.localScale.y,
                                                _maxChargeBarLength * (_charge.Value / 100f)
                                            );

        if (this.CompareTag("PCFlashLight")) OnChangeChargeBar?.Invoke(_charge.Value);
    }
}
