using System.Collections.Generic;
using System.Collections;

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

    [SerializeField] private AudioHapticSource firstLeftTeleporterTransitionHapticSource;
    [SerializeField] private AudioHapticSource firstRightTeleporterTransitionHapticSource;

    [SerializeField] private AudioHapticSource secondLeftTeleporterTransitionHapticSource;
    [SerializeField] private AudioHapticSource secondRightTeleporterTransitionHapticSource;

    [SerializeField] private AudioHapticSource pcLeftDamageHapticSource;
    [SerializeField] private AudioHapticSource pcRightDamageHapticSource;

    [SerializeField] private Transform[] leftButtons;
    private List<AudioSource> leftButtonAudios;
    private List<AudioHapticSource> leftButtonHapticSources;

    [SerializeField] private Transform[] rightButtons;
    private List<AudioSource> rightButtonAudios;
    private List<AudioHapticSource> rightButtonHapticSources;

    private bool _useTeleporterTimerHaptic;
    private bool _playTeleporterTransitionHaptic;
    private bool _useFirstTransitionHaptic;

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

        leftButtonAudios = new List<AudioSource>();
        leftButtonHapticSources = new List<AudioHapticSource>();
        for(int i = 0; i < leftButtons.Length; i++)
        {            
            leftButtonAudios.Add(leftButtons[i].GetComponent<AudioSource>());
            leftButtonHapticSources.Add(leftButtons[i].GetComponent<AudioHapticSource>());
        }

        rightButtonAudios = new List<AudioSource>();
        rightButtonHapticSources = new List<AudioHapticSource>();
        for(int i = 0; i < rightButtons.Length; i++)
        {
            rightButtonAudios.Add(rightButtons[i].GetComponent<AudioSource>());
            rightButtonHapticSources.Add(rightButtons[i].GetComponent<AudioHapticSource>());
        }

        _useAllButtonHaptics = -1;
        _isPaused = false;

        _hapticHealth = 1f;
        _playingHealthHaptic = false;

        _useTeleporterTimerHaptic = false;

        _playTeleporterTransitionHaptic = true;
        _useFirstTransitionHaptic = true;

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
        EventsManager.OnToggleAll += TogglePause;
        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;

        EventsManager.OnCreatedVRPlayer += CreatedVRPlayer;
        EventsManager.OnPingVRController += PingVRController;
        EventsManager.OnUseEnemyHaptic += UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic += UseBridgeHapticServerRpc;
        EventsManager.OnUseRopeHaptic += UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic += UseTeleporterTimerHaptic;
        EventsManager.OnTeleporterTransitionHaptic += TeleporterTransitionHaptic;

        EventsManager.OnUseButtonHaptic += UseButtonHapticServerRpc;
        EventsManager.OnUseAllButtonHaptic += UseAllButtonHapticServerRpc;

        EventsManager.OnIsNarratorSpeaking += IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterTimerHaptic += ActivateTeleporterTimerHaptic;
        EventsManager.OnChangeNarratorHaptic += ChangeNarratorHaptic;

        EventsManager.OnPlayNarratorHaptic += PlayNarratorHaptic;
        EventsManager.OnStopNarratorHaptic += StopNarratorHaptic;

        EventsManager.OnChangeHealthHaptic += ChangeHealthHaptic;
    }

    private void OnDisable()
    {
        EventsManager.OnToggleAll -= TogglePause;
        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;

        EventsManager.OnCreatedVRPlayer -= CreatedVRPlayer;
        EventsManager.OnPingVRController -= PingVRController;
        EventsManager.OnUseEnemyHaptic -= UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic -= UseBridgeHapticServerRpc;
        EventsManager.OnUseRopeHaptic -= UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic -= UseTeleporterTimerHaptic;
        EventsManager.OnTeleporterTransitionHaptic -= TeleporterTransitionHaptic;

        EventsManager.OnUseButtonHaptic -= UseButtonHapticServerRpc;
        EventsManager.OnUseAllButtonHaptic -= UseAllButtonHapticServerRpc;

        EventsManager.OnIsNarratorSpeaking -= IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterTimerHaptic -= ActivateTeleporterTimerHaptic;
        EventsManager.OnChangeNarratorHaptic -= ChangeNarratorHaptic;

        EventsManager.OnPlayNarratorHaptic -= PlayNarratorHaptic;
        EventsManager.OnStopNarratorHaptic -= StopNarratorHaptic;

        EventsManager.OnChangeHealthHaptic -= ChangeHealthHaptic;
    }

    private void ToggleAllAudioHapticSources(bool _toggle)
    {
        bridgeHaptic.enabled = _toggle;

        teleporterTimerHapticSource.enabled = _toggle;

        firstLeftTeleporterTransitionHapticSource.enabled = _toggle;
        firstRightTeleporterTransitionHapticSource.enabled = _toggle;

        secondLeftTeleporterTransitionHapticSource.enabled = _toggle;
        secondRightTeleporterTransitionHapticSource.enabled = _toggle;

        pcLeftDamageHapticSource.enabled = _toggle;
        pcRightDamageHapticSource.enabled = _toggle;

        for(int i = 0; i < leftButtonHapticSources.Count; i++)
        {            
            leftButtonHapticSources[i].enabled = _toggle;
        }

        for(int i = 0; i < rightButtonHapticSources.Count; i++)
        {            
            rightButtonHapticSources[i].enabled = _toggle;
        }
    }

    private void TogglePause(bool _toggle)
    {
        _isPaused = !_toggle;
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

    private void PlayNarratorHaptic()
    {
        Debug.Log("Play Narrator Haptic");
        if(narratorHapticSource != null)
        { 
            narratorHapticSource.enabled = true;
            narratorHapticSource.Play();
        }
    }

    private void StopNarratorHaptic()
    {
        if(narratorHapticSource != null)
        {
            narratorHapticSource.Stop();
            narratorHapticSource.enabled = false;
            narratorHapticSource = null;
        }
    }

    private void ChangeHealthHaptic(float _newHealth)
    {
        _hapticHealth = ((_newHealth / 100f));
        float _healthDelay = Mathf.Lerp(2.5f, 5f, _hapticHealth);

        if(!_playingHealthHaptic)
        { 
            PlayHealthHapticServerRpc(_healthDelay);
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

    private void PingVRController(int _controllerNum)
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseBridgeHapticServerRpc(float _bridgeMovementAmount)
    {
        if(_bridgeMovementAmount < 0) _bridgeMovementAmount *= -1;
        _intensity = Mathf.Clamp(_bridgeMovementAmount * 30, 0, 1);

        Debug.Log("Play Bridge Haptic");

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

    private void UseTeleporterTimerHaptic(float _newIntensity)
    {
        if(_isNarrator || !_useTeleporterTimerHaptic) return;
        _teleportIntensity = _newIntensity;
    }
    private void ActivateTeleporterTimerHaptic()
    {
        _useTeleporterTimerHaptic = true;
        PlayTeleporterTimerHapticServerRpc(2f);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayTeleporterTimerHapticServerRpc(float _delay)
    {
        StartCoroutine(PlayTeleporterTimerHaptic(_delay));
    }

    private IEnumerator PlayTeleporterTimerHaptic(float _delay)
    {
        Debug.Log("Play Teleport Timer Haptic");

        teleporterTimerHapticSource.enabled = true;
        teleporterTimerHapticSource.Stop();
        teleporterTimerHapticSource.SourceIntensity = _teleportIntensity;
        teleporterTimerHapticSource.PlayEventVibration();

        yield return new WaitForSeconds(_delay);

        if(_useTeleporterTimerHaptic) PlayTeleporterTimerHapticServerRpc(_delay);
    }





    private void TeleporterTransitionHaptic(float _newIntensity)
    {
        if(_playTeleporterTransitionHaptic)
        {
            PlayTeleporterTransitionHapticServerRpc(_newIntensity * 1.25f, 0.75f);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayTeleporterTransitionHapticServerRpc(float _newIntensity, float _teleporterTransitionDelay)
    {
        if(_useFirstTransitionHaptic)
        {
            StartCoroutine(PlayFirstTeleporterTransitionHaptic(_newIntensity, _teleporterTransitionDelay));
        }
        else
        {
            StartCoroutine(PlaySecondTeleporterTransitionHaptic(_newIntensity, _teleporterTransitionDelay));
        }

        _useFirstTransitionHaptic = !_useFirstTransitionHaptic;
    }

    private IEnumerator PlayFirstTeleporterTransitionHaptic(float _newIntensity, float _teleporterTransitionDelay)
    {
        _playTeleporterTransitionHaptic = false;

        Debug.Log("Play First Teleport Transition Haptic: intensity: " + _newIntensity + " : delay: " + _teleporterTransitionDelay);

        firstLeftTeleporterTransitionHapticSource.enabled = true;
        firstLeftTeleporterTransitionHapticSource.Stop();
        firstLeftTeleporterTransitionHapticSource.SourceIntensity = _newIntensity;
        firstLeftTeleporterTransitionHapticSource.PlayEventVibration();

        firstRightTeleporterTransitionHapticSource.enabled = true;
        firstRightTeleporterTransitionHapticSource.Stop();
        firstRightTeleporterTransitionHapticSource.SourceIntensity = _newIntensity;
        firstRightTeleporterTransitionHapticSource.PlayEventVibration();

        StartCoroutine(ControllerHapticDelay(leftControllerHaptic, _newIntensity, 0.1f));
        StartCoroutine(ControllerHapticDelay(rightControllerHaptic, _newIntensity, 0.1f));

        yield return new WaitForSeconds(_teleporterTransitionDelay);

        firstLeftTeleporterTransitionHapticSource.Stop();
        firstLeftTeleporterTransitionHapticSource.enabled = false;

        firstRightTeleporterTransitionHapticSource.Stop();
        firstRightTeleporterTransitionHapticSource.enabled = false;

        _playTeleporterTransitionHaptic = true;
    }

    private IEnumerator PlaySecondTeleporterTransitionHaptic(float _newIntensity, float _teleporterTransitionDelay)
    {
        _playTeleporterTransitionHaptic = false;

        Debug.Log("Play Second Teleport Transition Haptic: intensity: " + _newIntensity + " : delay: " + _teleporterTransitionDelay);

        secondLeftTeleporterTransitionHapticSource.enabled = true;
        secondLeftTeleporterTransitionHapticSource.Stop();
        secondLeftTeleporterTransitionHapticSource.SourceIntensity = _newIntensity;
        secondLeftTeleporterTransitionHapticSource.PlayEventVibration();

        secondLeftTeleporterTransitionHapticSource.enabled = true;
        secondLeftTeleporterTransitionHapticSource.Stop();
        secondLeftTeleporterTransitionHapticSource.SourceIntensity = _newIntensity;
        secondLeftTeleporterTransitionHapticSource.PlayEventVibration();

        StartCoroutine(ControllerHapticDelay(leftControllerHaptic, _newIntensity, 0.1f));
        StartCoroutine(ControllerHapticDelay(rightControllerHaptic, _newIntensity, 0.1f));

        yield return new WaitForSeconds(_teleporterTransitionDelay);

        secondLeftTeleporterTransitionHapticSource.Stop();
        secondLeftTeleporterTransitionHapticSource.enabled = false;

        secondLeftTeleporterTransitionHapticSource.Stop();
        secondLeftTeleporterTransitionHapticSource.enabled = false;

        _playTeleporterTransitionHaptic = true;
    }





    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseButtonHapticServerRpc(int _buttonNum)
    {
        for(int i = 0; i < leftButtonHapticSources.Count; i++)
        {
            leftButtonHapticSources[i].Stop();
            leftButtonHapticSources[i].enabled = false;

            rightButtonHapticSources[i].Stop();
            rightButtonHapticSources[i].enabled = false;
        }
        
        Debug.Log("Play Specific Button Haptic");

        leftButtonAudios[_buttonNum].volume = 0f;
        leftButtonHapticSources[_buttonNum].enabled = true;
        leftButtonHapticSources[_buttonNum].SourceIntensity = 1f;
        leftButtonHapticSources[_buttonNum].PlayEventVibration();

        rightButtonAudios[_buttonNum].volume = 0f;
        rightButtonHapticSources[_buttonNum].enabled = true;
        rightButtonHapticSources[_buttonNum].SourceIntensity = 1f;
        rightButtonHapticSources[_buttonNum].PlayEventVibration();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseAllButtonHapticServerRpc()
    {
        leftButtonHapticSources[0].Stop();
        rightButtonHapticSources[0].Stop();
        
        Debug.Log("Play First All Buttons Haptic");

        leftButtonAudios[0].volume = 1f;
        leftButtonHapticSources[0].enabled = true;
        leftButtonHapticSources[0].SourceIntensity = 1f;
        leftButtonHapticSources[0].Play();

        rightButtonAudios[0].volume = 1f;
        rightButtonHapticSources[0].enabled = true;
        rightButtonHapticSources[0].SourceIntensity = 1f;
        rightButtonHapticSources[0].Play();

        _useAllButtonHaptics = 1;
    }

    private void Update()
    {
        if(_useAllButtonHaptics != -1)
        {
            if(_playFullButtonsHaptics && leftButtonAudios[_useAllButtonHaptics - 1].isPlaying == false && rightButtonAudios[_useAllButtonHaptics - 1].isPlaying == false)
            {
                StartCoroutine(FullButtonHapticsDelay(0.5f));
            }
        }
    }

    IEnumerator FullButtonHapticsDelay(float _delay)
    {
        _playFullButtonsHaptics = false;

        yield return new WaitForSeconds(_delay);

        leftButtonHapticSources[_useAllButtonHaptics].enabled = true;
        leftButtonHapticSources[_useAllButtonHaptics].Stop();

        rightButtonHapticSources[_useAllButtonHaptics].enabled = true;
        rightButtonHapticSources[_useAllButtonHaptics].Stop();

        Debug.Log("Play Remaining All Buttons Haptic");

        leftButtonAudios[_useAllButtonHaptics].volume = 1f;
        leftButtonHapticSources[_useAllButtonHaptics].SourceIntensity = 1f;
        leftButtonHapticSources[_useAllButtonHaptics].Play();

        rightButtonAudios[_useAllButtonHaptics].volume = 1f;
        rightButtonHapticSources[_useAllButtonHaptics].SourceIntensity = 1f;
        rightButtonHapticSources[_useAllButtonHaptics].Play();

        if((_useAllButtonHaptics + 1) < leftButtonHapticSources.Count)
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayHealthHapticServerRpc(float _delay)
    {
        StartCoroutine(PlayHealthHaptic(_delay));
    }

    private IEnumerator PlayHealthHaptic(float _delay)
    {
        _playingHealthHaptic = true;

        pcLeftDamageHapticSource.enabled = true;
        pcLeftDamageHapticSource.Stop();

        pcRightDamageHapticSource.enabled = true;
        pcRightDamageHapticSource.Stop();

        pcLeftDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;
        pcRightDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;

        Debug.Log("Play Left Health Haptic");
        pcLeftDamageHapticSource.PlayEventVibration();

        Debug.Log("Play Right Health Haptic");
        pcRightDamageHapticSource.PlayEventVibration();

        tunnel.defaultParameters.apertureSize = (1f - _hapticHealth);

        yield return new WaitForSeconds(_delay);

        if(_hapticHealth < 1f)
        {
            _hapticHealth = _hapticHealth + 0.05f;
            float _healthDelay = Mathf.Lerp(2.5f, 5f, _hapticHealth);
            PlayHealthHapticServerRpc(_healthDelay);
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