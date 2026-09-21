using System.Collections.Generic;
using System.Collections;

using Interhaptics;
using Interhaptics.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using Interhaptics.Core;

using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

using Unity.Netcode;

public class HapticManager : MonoBehaviour
{
    public Transform leftHeadListener;
    public Transform rightHeadListener;

    public Transform narratorLeftHeadTransform;
    private AudioHapticSource narratorLeftHeadHaptic;
    public Transform narratorRightHeadTransform;
    private AudioHapticSource narratorRightHeadHaptic;
    [SerializeField] private AudioHapticSource bridgeHaptic;

    private float _intensity;
    private float _teleportIntensity;

    public Transform leftController;
    private HapticImpulsePlayer leftControllerHaptic;
    public Transform rightController;
    private HapticImpulsePlayer rightControllerHaptic;

    private bool _canUseHaptics;
    private bool _isUsingEnemyHaptic;

    [SerializeField] private TunnelingVignetteController tunnel;

    [Header("Haptic Sources")]
    [SerializeField] private AudioHapticSource teleporterHapticSource;
    [SerializeField] private AudioHapticSource pcLeftDamageHapticSource;
    [SerializeField] private AudioHapticSource pcRightDamageHapticSource;

    [SerializeField] private Transform[] leftButtons;
    private List<AudioSource> leftButtonAudios;
    private List<AudioHapticSource> leftButtonHapticSources;

    [SerializeField] private Transform[] rightButtons;
    private List<AudioSource> rightButtonAudios;
    private List<AudioHapticSource> rightButtonHapticSources;

    private bool _useTeleporterHaptic;

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

        _useTeleporterHaptic = false;

        _isNarrator = false;

        _isUsingEnemyHaptic = false;

        narratorLeftHeadHaptic = narratorLeftHeadTransform.GetComponent<AudioHapticSource>();
        narratorRightHeadHaptic = narratorRightHeadTransform.GetComponent<AudioHapticSource>();

        leftControllerHaptic = leftController.GetComponent<HapticImpulsePlayer>();
        rightControllerHaptic = rightController.GetComponent<HapticImpulsePlayer>();

        // StartCoroutine(PlayEnemyHaptic(2f));
    }

    private void OnEnable()
    {
        EventsManager.OnToggleAll += TogglePause;

        EventsManager.OnCreatedVRPlayer += CreatedVRPlayer;
        EventsManager.OnPingVRController += PingVRController;
        EventsManager.OnUseEnemyHaptic += UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic += UseBridgeHapticServerRpc;
        EventsManager.OnUseRopeHaptic += UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic += UseTeleportHaptic;
        EventsManager.OnUseButtonHaptic += UseButtonHapticServerRpc;
        EventsManager.OnUseAllButtonHaptic += UseAllButtonHapticServerRpc;

        EventsManager.OnIsNarratorSpeaking += IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterHaptic += ActivateTeleporterHaptic;

        EventsManager.OnChangeHealthHaptic += ChangeHealthHaptic;
    }

    private void OnDisable()
    {
        EventsManager.OnToggleAll -= TogglePause;

        EventsManager.OnCreatedVRPlayer -= CreatedVRPlayer;
        EventsManager.OnPingVRController -= PingVRController;
        EventsManager.OnUseEnemyHaptic -= UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic -= UseBridgeHapticServerRpc;
        EventsManager.OnUseRopeHaptic -= UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic -= UseTeleportHaptic;
        EventsManager.OnUseButtonHaptic -= UseButtonHapticServerRpc;
        EventsManager.OnUseAllButtonHaptic -= UseAllButtonHapticServerRpc;

        EventsManager.OnIsNarratorSpeaking -= IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterHaptic -= ActivateTeleporterHaptic;

        EventsManager.OnChangeHealthHaptic -= ChangeHealthHaptic;
    }

    private void TogglePause(bool _toggle)
    {
        _isPaused = !_toggle;
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

    private void ActivateTeleporterHaptic()
    {
        Debug.Log("ActivateTeleporterHaptic");
        _useTeleporterHaptic = true;
        PlayTeleportHapticServerRpc(2f);
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

        Debug.Log("bridge Haptic");

        bridgeHaptic.Stop();
        bridgeHaptic.SourceIntensity = _intensity;        
        bridgeHaptic.PlayEventVibration();
    }

    private void UseRopeHaptic(int _controllerNum, float _ropeDistance)
    {
        switch(_controllerNum)
        {
            case 0:
                leftControllerHaptic.SendHapticImpulse(_ropeDistance, 0.1f);
                break;
            case 1:
                rightControllerHaptic.SendHapticImpulse(_ropeDistance, 0.1f);
                break;
        }
    }

    private void UseTeleportHaptic(float _newIntensity)
    {
        if(_isNarrator || !_useTeleporterHaptic) return;
        _teleportIntensity = _newIntensity;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UseButtonHapticServerRpc(int _buttonNum)
    {
        for(int i = 0; i < leftButtonHapticSources.Count; i++)
        {
            leftButtonHapticSources[i].Stop();
            rightButtonHapticSources[i].Stop();
        }
        
        leftButtonAudios[_buttonNum].volume = 0f;
        leftButtonHapticSources[_buttonNum].SourceIntensity = 1f;
        leftButtonHapticSources[_buttonNum].PlayEventVibration();

        rightButtonAudios[_buttonNum].volume = 0f;
        rightButtonHapticSources[_buttonNum].SourceIntensity = 1f;
        rightButtonHapticSources[_buttonNum].PlayEventVibration();
    }

    private void UseAllButtonHapticServerRpc()
    {
        Debug.Log("UseAllButtonHapticServerRpc");

        leftButtonHapticSources[0].Stop();
        rightButtonHapticSources[0].Stop();
        
        leftButtonAudios[0].volume = 1f;
        leftButtonHapticSources[0].SourceIntensity = 1f;
        leftButtonHapticSources[0].Play();

        rightButtonAudios[0].volume = 1f;
        rightButtonHapticSources[0].SourceIntensity = 1f;
        rightButtonHapticSources[0].Play();

        _useAllButtonHaptics = 1;
    }

    private void Update()
    {
        // if(!_canUseHaptics) return;

        if(_useAllButtonHaptics != -1)
        {
            Debug.Log("leftButtonAudios[" + (_useAllButtonHaptics - 1) + "].isPlaying: " + leftButtonAudios[_useAllButtonHaptics - 1].isPlaying);
            Debug.Log("rightButtonAudios[" + (_useAllButtonHaptics - 1) + "].isPlaying: " + rightButtonAudios[_useAllButtonHaptics - 1].isPlaying);
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

        leftButtonHapticSources[_useAllButtonHaptics].Stop();
        rightButtonHapticSources[_useAllButtonHaptics].Stop();

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

            _controller.SendHapticImpulse((_delay - elapsed) / _delay, 0.1f);

            yield return null;
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayTeleportHapticServerRpc(float _delay)
    {
        StartCoroutine(PlayTeleportHaptic(_delay));
    }

    private IEnumerator PlayTeleportHaptic(float _delay)
    {
        Debug.Log("teleport Haptic");

        teleporterHapticSource.Stop();
        teleporterHapticSource.SourceIntensity = _teleportIntensity;
        teleporterHapticSource.PlayEventVibration();

        yield return new WaitForSeconds(_delay);

        if(_useTeleporterHaptic) PlayTeleportHapticServerRpc(_delay);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayHealthHapticServerRpc(float _delay)
    {
        StartCoroutine(PlayHealthHaptic(_delay));
    }

    private IEnumerator PlayHealthHaptic(float _delay)
    {
        _playingHealthHaptic = true;

        pcLeftDamageHapticSource.Stop();
        pcRightDamageHapticSource.Stop();

        pcLeftDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;
        pcRightDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 1.5f;

        Debug.Log("health left Haptic");
        pcLeftDamageHapticSource.PlayEventVibration();

        Debug.Log("health right Haptic");
        pcRightDamageHapticSource.PlayEventVibration();

        tunnel.defaultParameters.apertureSize = (1f - _hapticHealth);

        yield return new WaitForSeconds(_delay);

        if(_hapticHealth < 1f)
        {
            _hapticHealth = _hapticHealth + 0.05f;
            float _healthDelay = Mathf.Lerp(2.5f, 5f, _hapticHealth);
             Debug.Log("2 _hapticHealth: " + _hapticHealth + " : 2 _healthDelay : " + _healthDelay);
            PlayHealthHapticServerRpc(_healthDelay);
        }
        else
        {
            _playingHealthHaptic = false;
        }
    }
}