using System.Collections;
using System.Collections.Generic;

using Interhaptics;
using Interhaptics.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using Interhaptics.Core;

using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

using Unity.Netcode;

public class HapticManager : NetworkBehaviour
{
    public Transform leftHeadListener;
    public Transform rightHeadListener;

    // public Transform narratorLeftHeadTransform;
    // private AudioHapticSource narratorLeftHeadHaptic;
    // public Transform narratorRightHeadTransform;
    // private AudioHapticSource narratorRightHeadHaptic;

    private float _intensity;
    private float _teleportIntensity;

    public Transform leftController;
    private HapticImpulsePlayer leftControllerHaptic;
    public Transform rightController;
    private HapticImpulsePlayer rightControllerHaptic;

    private bool _canUseHaptics;
    private bool _isUsingEnemyHaptic;

    [SerializeField] private TunnelingVignetteController tunnel;

    private AudioHapticSource narratorHapticSource;

    [Header("Haptic Sources")]
    [SerializeField] private AudioHapticSource bridgeHaptic;
    [SerializeField] private AudioHapticSource teleporterTimerHapticSource;

    [SerializeField] private AudioHapticSource teleporterTransitionHapticSource;

    [SerializeField] private AudioHapticSource pcLeftDamageHapticSource;
    [SerializeField] private AudioHapticSource pcRightDamageHapticSource;

    [SerializeField] private Transform[] stoneButtons;
    private List<AudioHapticSource> stoneButtonHapticSources;

    private bool _useTeleporterTimerHaptic;

    private bool _isNarrator;

    private float _hapticHealth;
    private bool _playingHealthHaptic;

    private bool _isPaused;
    private int _useAllButtonHaptics;
    private bool _playFullButtonsHaptics;

    // Start is called before the first frame update
    void Start()
    {
        _playFullButtonsHaptics = true;

        stoneButtonHapticSources = new List<AudioHapticSource>();
        for(int i = 0; i < stoneButtons.Length; i++)
        {            
            stoneButtonHapticSources.Add(stoneButtons[i].GetComponent<AudioHapticSource>());
        }

        _useAllButtonHaptics = -1;
        _isPaused = false;

        _hapticHealth = 1f;
        _playingHealthHaptic = false;

        _useTeleporterTimerHaptic = false;

        _isNarrator = false;

        _isUsingEnemyHaptic = false;

        // narratorLeftHeadHaptic = narratorLeftHeadTransform.GetComponent<AudioHapticSource>();
        // narratorRightHeadHaptic = narratorRightHeadTransform.GetComponent<AudioHapticSource>();

        leftControllerHaptic = leftController.GetComponent<HapticImpulsePlayer>();
        rightControllerHaptic = rightController.GetComponent<HapticImpulsePlayer>();

        // StartCoroutine(PlayEnemyHaptic(2f));
        ToggleAllAudioHapticSources(false);
    }

    private void OnEnable()
    {
        EventsManager.OnToggleAll += TogglePauseRpc;
        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;

        EventsManager.OnCreatedVRPlayer += CreatedVRPlayer;
        EventsManager.OnPingVRController += PingVRControllerRpc;
        EventsManager.OnUseEnemyHaptic += UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic += UseBridgeHapticRpc;
        EventsManager.OnUseRopeHaptic += UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic += UseTeleporterTimerHapticRpc;
        EventsManager.OnTeleporterTransitionHaptic += TeleporterTransitionHapticRpc;

        EventsManager.OnUseButtonHaptic += UseButtonHapticRpc;
        EventsManager.OnUseAllButtonHaptic += UseAllButtonHapticRpc;

        EventsManager.OnIsNarratorSpeaking += IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterTimerHaptic += ActivateTeleporterTimerHapticRpc;

        EventsManager.OnStartWithInstantTeleport += DeactivateTeleporterTimerHapticRpc;
        EventsManager.OnEverythingCollected += DeactivateTeleporterTimerHapticRpc;

        EventsManager.OnChangeNarratorHaptic += ChangeNarratorHaptic;

        EventsManager.OnPlayNarratorHaptic += PlayNarratorHapticRpc;
        EventsManager.OnStopNarratorHaptic += StopNarratorHapticRpc;

        EventsManager.OnChangeHealthHaptic += ChangeHealthHapticRpc;

        EventsManager.OnPlayStoneButtonHaptic += PlayStoneButtonHaptic;
    }

    private void OnDisable()
    {
        EventsManager.OnToggleAll -= TogglePauseRpc;
        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;

        EventsManager.OnCreatedVRPlayer -= CreatedVRPlayer;
        EventsManager.OnPingVRController -= PingVRControllerRpc;
        EventsManager.OnUseEnemyHaptic -= UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic -= UseBridgeHapticRpc;
        EventsManager.OnUseRopeHaptic -= UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic -= UseTeleporterTimerHapticRpc;
        EventsManager.OnTeleporterTransitionHaptic -= TeleporterTransitionHapticRpc;

        EventsManager.OnUseButtonHaptic -= UseButtonHapticRpc;
        EventsManager.OnUseAllButtonHaptic -= UseAllButtonHapticRpc;

        EventsManager.OnIsNarratorSpeaking -= IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterTimerHaptic -= ActivateTeleporterTimerHapticRpc;

        EventsManager.OnStartWithInstantTeleport -= DeactivateTeleporterTimerHapticRpc;
        EventsManager.OnEverythingCollected -= DeactivateTeleporterTimerHapticRpc;

        EventsManager.OnChangeNarratorHaptic -= ChangeNarratorHaptic;

        EventsManager.OnPlayNarratorHaptic -= PlayNarratorHapticRpc;
        EventsManager.OnStopNarratorHaptic -= StopNarratorHapticRpc;

        EventsManager.OnChangeHealthHaptic -= ChangeHealthHapticRpc;

        EventsManager.OnPlayStoneButtonHaptic -= PlayStoneButtonHaptic;
    }

    private void ToggleAllAudioHapticSources(bool _toggle)
    {
        if(!_toggle) bridgeHaptic.Stop();
        bridgeHaptic.enabled = _toggle;

        if(_toggle)
        {
            teleporterTimerHapticSource.enabled = _toggle;
            if(IsOwner) ActivateTeleporterTimerHapticRpc();
        }else{
            teleporterTimerHapticSource.Stop();
            teleporterTimerHapticSource.enabled = _toggle;
        }

        if(!_toggle) teleporterTransitionHapticSource.Stop();
        teleporterTransitionHapticSource.enabled = _toggle;

        if(!_toggle) pcLeftDamageHapticSource.Stop();
        pcLeftDamageHapticSource.enabled = _toggle;
        if(!_toggle) pcRightDamageHapticSource.Stop();
        pcRightDamageHapticSource.enabled = _toggle;

        for(int i = 0; i < stoneButtonHapticSources.Count; i++)
        {            
            if(!_toggle) stoneButtonHapticSources[i].Stop();
            stoneButtonHapticSources[i].enabled = _toggle;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void TogglePauseRpc(bool _toggle)
    {
        _isPaused = !_toggle;
        ToggleAllAudioHapticSources(_isPaused);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TogglePauseManagerAudioRpc(bool _toggle)
    {
        if(narratorHapticSource == null) return;
        if(_toggle)
        {
            narratorHapticSource.Stop(); 
            narratorHapticSource.enabled = false;
        }
    }

    private void ChangeNarratorHaptic(AudioHapticSource _newHapticSource)
    {
        if(narratorHapticSource != null)
        {
            narratorHapticSource.Stop();
            narratorHapticSource.enabled = false;
        }

        if(_newHapticSource != null)
        {
            narratorHapticSource = _newHapticSource;
            narratorHapticSource.enabled = true;
            narratorHapticSource.Stop();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayNarratorHapticRpc()
    {
        if(narratorHapticSource != null)
        { 
            narratorHapticSource.enabled = true;
            narratorHapticSource.Play();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void StopNarratorHapticRpc()
    {
        if(narratorHapticSource != null)
        {
            narratorHapticSource.Stop();
            narratorHapticSource.enabled = false;
            narratorHapticSource = null;
        }
    }

    private void ChangeHealthHapticRpc(float _newHealth)
    {
        _hapticHealth = _newHealth / 100f;
        float _healthDelay = Mathf.Lerp(2.5f, 5f, _hapticHealth);

        if(IsOwner && !_playingHealthHaptic)
        { 
            Debug.Log("ChangeHealthHapticRpc: " + _newHealth);
            Debug.Log("1 PlayHealthHapticRpc: " + _healthDelay);
            PlayHealthHapticRpc(_healthDelay);
        }
    }

    private void IsNarratorSpeaking(bool _isNewNarrator)
    {
        _isNarrator = _isNewNarrator;
    }

    private void CreatedVRPlayer()
    {
        _canUseHaptics = true;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PingVRControllerRpc(int _controllerNum)
    {
        switch(_controllerNum)
        {
            case 0:
                StartCoroutine(Ping(leftControllerHaptic, 0.25f));
                break;
            case 1:
                StartCoroutine(Ping(rightControllerHaptic, 0.25f));
                break;
        }
    }

    private void UseEnemyHaptic(bool _useHaptic)
    {
        // if(_isUsingEnemyHaptic != _useHaptic)
        // {
        //     _isUsingEnemyHaptic = _useHaptic;
        //     // if(_useHaptic) StartCoroutine(PlayEnemyHaptic(2f));
        // }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseBridgeHapticRpc(float _bridgeMovementAmount)
    {
        if(_bridgeMovementAmount < 0) _bridgeMovementAmount *= -1;
        _intensity = Mathf.Clamp(_bridgeMovementAmount * 30, 0, 1);

        bridgeHaptic.enabled = true;
        bridgeHaptic.Stop();
        bridgeHaptic.SourceIntensity = _intensity;        
        bridgeHaptic.PlayEventVibration();
    }

    private void UseRopeHaptic(int _controllerNum, float _ropeDistance)
    {
        switch(_controllerNum)
        {
            case 0:
                StartCoroutine(ControllerHapticDelay(leftControllerHaptic, _ropeDistance, 0.1f));
                break;
            case 1:
                StartCoroutine(ControllerHapticDelay(rightControllerHaptic, _ropeDistance, 0.1f));
                break;
        }
    }

    private IEnumerator ControllerHapticDelay(HapticImpulsePlayer _controller, float _intensity, float _delay)
    {
        _controller.SendHapticImpulse(_intensity, _delay);
        yield return new WaitForSeconds(_delay);
        _controller.SendHapticImpulse(0, _delay);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseTeleporterTimerHapticRpc(float _newIntensity)
    {
        if(_isNarrator || !_useTeleporterTimerHaptic || _isPaused) return;
        _teleportIntensity = _newIntensity;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ActivateTeleporterTimerHapticRpc()
    {
        // if(_isPaused) return;
        _useTeleporterTimerHaptic = true;
        if(IsOwner) PlayTeleporterTimerHapticRpc(2f);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayTeleporterTimerHapticRpc(float _delay)
    {
        // if(_isPaused) return;
        StartCoroutine(PlayTeleporterTimerHaptic(_delay));
    }

    private IEnumerator PlayTeleporterTimerHaptic(float _delay)
    {
        if(!_isPaused)
        {
            teleporterTimerHapticSource.enabled = true;
            teleporterTimerHapticSource.Stop();
            teleporterTimerHapticSource.SourceIntensity = _teleportIntensity;
            teleporterTimerHapticSource.PlayEventVibration();
        }

        yield return new WaitForSeconds(_delay);

        if(_useTeleporterTimerHaptic && IsOwner) PlayTeleporterTimerHapticRpc(_delay);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void DeactivateTeleporterTimerHapticRpc()
    {
        _useTeleporterTimerHaptic = false;
        teleporterTimerHapticSource.Stop();
        teleporterTimerHapticSource.enabled = false;
    }




    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void TeleporterTransitionHapticRpc(float _newIntensity)
    {
        if(_newIntensity < 0.01f) _newIntensity = 0.01f;

        if(teleporterTransitionHapticSource.enabled == false) teleporterTransitionHapticSource.enabled = true;
        teleporterTransitionHapticSource.SourceIntensity = _newIntensity;
        if(!teleporterTransitionHapticSource.isPlaying) teleporterTransitionHapticSource.PlayEventVibration();

        StartCoroutine(ControllerHapticDelay(leftControllerHaptic, _newIntensity, 0.1f));
        StartCoroutine(ControllerHapticDelay(rightControllerHaptic, _newIntensity, 0.1f));
    }

    private void PlayStoneButtonHaptic(AudioHapticSource _haptic)
    {
        Debug.Log("PlayStoneButtonHaptic: " + _haptic);
        _haptic.Stop();
        _haptic.enabled = true;
        _haptic.SourceIntensity = 1f;
        _haptic.PlayEventVibration();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseButtonHapticRpc(int _buttonNum)
    {
        for(int i = 0; i < stoneButtonHapticSources.Count; i++)
        {
            stoneButtonHapticSources[i].Stop();
            stoneButtonHapticSources[i].enabled = false;
        }
        
        stoneButtonHapticSources[_buttonNum].enabled = true;
        stoneButtonHapticSources[_buttonNum].SourceIntensity = 1f;
        stoneButtonHapticSources[_buttonNum].PlayEventVibration();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseAllButtonHapticRpc()
    {
        stoneButtonHapticSources[0].Stop();
        
        stoneButtonHapticSources[0].enabled = true;
        stoneButtonHapticSources[0].SourceIntensity = 1f;
        stoneButtonHapticSources[0].Play();

        _useAllButtonHaptics = 1;
    }

    private void Update()
    {
        if(_useAllButtonHaptics != -1)
        {
            if(_playFullButtonsHaptics)
            {
                FullButtonHapticsRpc(0.5f);
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void FullButtonHapticsRpc(float _delay)
    {
        StartCoroutine(FullButtonHapticsDelay(_delay));
    }

    IEnumerator FullButtonHapticsDelay(float _delay)
    {
        _playFullButtonsHaptics = false;

        yield return new WaitForSeconds(_delay);

        stoneButtonHapticSources[_useAllButtonHaptics].enabled = true;
        stoneButtonHapticSources[_useAllButtonHaptics].Stop();

        stoneButtonHapticSources[_useAllButtonHaptics].SourceIntensity = 1f;
        stoneButtonHapticSources[_useAllButtonHaptics].Play();

        if((_useAllButtonHaptics + 1) < stoneButtonHapticSources.Count)
        {
            _useAllButtonHaptics++;
        }
        else
        {
            _useAllButtonHaptics = -1;
        }

        _playFullButtonsHaptics = true;
    }

    IEnumerator Ping(HapticImpulsePlayer _controller, float _delay)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            StartCoroutine(ControllerHapticDelay(_controller, (_delay - elapsed) / _delay, 0.1f));

            yield return null;
        }

        // _controller.SendHapticImpulse(0f, 0.1f);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayHealthHapticRpc(float _delay)
    {
        Debug.Log("Function PlayHealthHapticRpc: " + _delay);
        StartCoroutine(PlayHealthHaptic(_delay));
    }

    private IEnumerator PlayHealthHaptic(float _delay)
    {
        Debug.Log("PlayHealthHaptic: " + _delay);

        _playingHealthHaptic = true;

        pcLeftDamageHapticSource.enabled = true;
        pcLeftDamageHapticSource.Stop();

        pcRightDamageHapticSource.enabled = true;
        pcRightDamageHapticSource.Stop();

        pcLeftDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;
        pcRightDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;

        pcLeftDamageHapticSource.PlayEventVibration();
        pcRightDamageHapticSource.PlayEventVibration();

        tunnel.defaultParameters.apertureSize = (1f - _hapticHealth);

        yield return new WaitForSeconds(_delay);

        if(_hapticHealth < 1f)
        {
            _hapticHealth = _hapticHealth + 0.05f;
            float _healthDelay = Mathf.Lerp(2.5f, 5f, _hapticHealth);
            Debug.Log("2 PlayHealthHapticRpc: " + _healthDelay);
            PlayHealthHapticRpc(_healthDelay);
        }
        else
        {
            _playingHealthHaptic = false;
        }
    }
}


    // private void Update()
    // {
    //     if(!_canUseHaptics) return;

    //     // _intensity = 1f - Mathf.Clamp(Vector3.Distance(listenPoint.position, leftController.position) * (1f / 6f), 0, 1);
    //     // leftControllerHaptic.SendHapticImpulse(_intensity, 0.1f);

    //     // _intensity = 1f - Mathf.Clamp(Vector3.Distance(listenPoint.position, rightController.position) * (1f / 6f), 0, 1);
    //     // rightControllerHaptic.SendHapticImpulse(_intensity, 0.1f);
    // }

    // private IEnumerator PlayEnemyHaptic(float _delay)
    // {
    //     _intensity = 1f - Mathf.Clamp(Vector3.Distance(leftHeadListener.position, narratorLeftHeadTransform.position) * (1f / 20f), 0, 1);
    //     narratorLeftHeadHaptic.SourceIntensity = _intensity;

    //     _intensity = 1f - Mathf.Clamp(Vector3.Distance(rightHeadListener.position, narratorRightHeadTransform.position) * (1f / 20f), 0, 1);
    //     narratorRightHeadHaptic.SourceIntensity = _intensity;
        
    //     narratorLeftHeadHaptic.PlayEventVibration();
    //     narratorRightHeadHaptic.PlayEventVibration();

    //     yield return new WaitForSeconds(_delay);

    //     // if(_isUsingEnemyHaptic) StartCoroutine(PlayEnemyHaptic(_delay));
    // }