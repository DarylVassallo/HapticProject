using UnityEngine;
using UnityEngine.InputSystem;
using System;

using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

//This script controls the flashlight's charge, as it runs out, the strength and range is also reduced. It can be charged, but the flash light is off during it.
public class FlashlightCharge : NetworkBehaviour
{
    private NetworkVariable<bool> _flashLightToggle = new (true);

    private NetworkVariable<float> _charge = new (100f);
    private NetworkVariable<bool> _isCharging = new (false);

    [SerializeField] private float decayRate;
    [SerializeField] private float chargeRate;
    [SerializeField] private float flashLightRotationSpeed;

    // [SerializeField] private float maxFlashLightIntensity;
    // public float currentFlashLightIntensity;
    [SerializeField] private float maxFlashLightRange;
    public float currentFlashLightRange;

    [SerializeField] private float chargeIntensity;

    [SerializeField] private Transform flashlightLever;
    
    [SerializeField] private Transform chargeBar;
    private float _maxChargeBarLength;

    public static event Action<float> OnChangeChargeBar;

    private bool _isNetworkSpawned = false;

    private bool _shuttingDown = true;

    [SerializeField] private AudioClip handCrankAudio;
    private AudioSource _audioSource;

    [SerializeField] private Vector3 _initialPosition;

    private void Awake()
    {
        // _initialPosition = this.transform.position;

        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _maxChargeBarLength = chargeBar.localScale.z;
    }
    
    private void OnEnable()
    {
        PCPlayerInputManager.OnFire += ChargeFlashlightWithMouse;
        PCPlayerInputManager.OnFire2 += SetFlashlightEnableServerRpc;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnFire -= ChargeFlashlightWithMouse;
        PCPlayerInputManager.OnFire2 -= SetFlashlightEnableServerRpc;
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

    //Plays charge flashlight audio when required
    [ClientRpc]
    private void PlayAudioClientRpc(int _audioNum)
    {
        AudioClip _currentAudio = handCrankAudio;

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = _currentAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 
        }
    }

    //Stops audio
    [ClientRpc]
    private void StopAudioClientRpc()
    {
        _audioSource.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetFlashlightEnableServerRpc()
    {
        _flashLightToggle.Value = !_flashLightToggle.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIsChargingServerRpc(bool _newIsCharging)
    {
        _isCharging.Value = _newIsCharging;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetChargeServerRpc(float _newCharge)
    {
        _charge.Value = _newCharge;
    }

    //This triggers if the PC Player pressed the left mouse button.
    // This allows it to begin charging the PC Flashlight
    private void ChargeFlashlightWithMouse(InputAction.CallbackContext context)
    {
        if(_isNetworkSpawned) SetIsChargingServerRpc(context.performed);
        ChangeFlashLightStrength(0, _isCharging.Value ? chargeIntensity : 1f);
    }

    private void FixedUpdate()
    {              
        //Stops any audio if the flashlight is fully charged, is 'trying' to charge, and is not off
        if(_charge.Value >= 100 && _isCharging.Value && !_shuttingDown) StopAudioClientRpc();

        //Prevents any calculations if the flashlight is fully charged and is 'trying' to charge, or has no charge and 
        if ((_charge.Value >= 100 && _isCharging.Value) || (_charge.Value <= 0 && !_isCharging.Value && _flashLightToggle.Value)) return;

        //Charges the flashlight, if it is trying to, and is able to do so
        if (_isCharging.Value && _charge.Value < 100)
        {
            if(!_flashLightToggle.Value) SetFlashlightEnableServerRpc();
            ChangeFlashLightStrength(chargeRate, _isCharging.Value ? chargeIntensity : 1f);

            if(flashlightLever != null)
            {
                flashlightLever.Rotate(Vector3.forward * Time.deltaTime * flashLightRotationSpeed);
                if(!_shuttingDown) PlayAudioClientRpc(0);
            }
        }else{
            //Causes the flashlight's charge to decay, if it is not charging
        
            ChangeFlashLightStrength(-decayRate, _isCharging.Value ? chargeIntensity : 1f);
            if(!_shuttingDown) StopAudioClientRpc();
        }
    }

    //This changes the flashlight's strength, either to slowly charge or decay it
    private void ChangeFlashLightStrength(float _change, float _brightness)
    {        
        if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(_charge.Value + _change);

        if(_charge.Value < 0)
        {
            if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(0f);
        }else if(_charge.Value > 100)
        { 
            if(_isNetworkSpawned && !_shuttingDown) SetChargeServerRpc(100f);
        }

        currentFlashLightRange = maxFlashLightRange * (_charge.Value / 100f) * _brightness;

        chargeBar.localScale = new Vector3  (   chargeBar.localScale.x, 
                                                chargeBar.localScale.y,
                                                _maxChargeBarLength * (_charge.Value / 100f)
                                            );

        if (this.CompareTag("PCFlashLight")) OnChangeChargeBar?.Invoke(_charge.Value);
    }
}
