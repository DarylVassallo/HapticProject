using System.Collections;
using Interhaptics;
using Interhaptics.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

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

    [SerializeField] private HapticMaterial teleportBarHaptic;
    [SerializeField] private HapticMaterial buttonHaptic;

    [Header("Haptic Sources")]
    [SerializeField] private AudioHapticSource teleporterHapticSource;
    [SerializeField] private AudioHapticSource pcDamageHapticSource;
    [SerializeField] private AudioHapticSource leftButtonHapticSource;
    [SerializeField] private AudioHapticSource rightButtonHapticSource;

    private bool _useTeleporterHaptic;

    private bool _isNarrator;

    private float _hapticHealth;
    private bool _playingHealthHaptic;

    // Start is called before the first frame update
    void Start()
    {
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
        EventsManager.OnCreatedVRPlayer += CreatedVRPlayer;
        EventsManager.OnPingVRController += PingVRController;
        EventsManager.OnUseEnemyHaptic += UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic += UseBridgeHaptic;
        EventsManager.OnUseRopeHaptic += UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic += UseTeleportHaptic;
        EventsManager.OnUseButtonHaptic += UseButtonHaptic;

        EventsManager.OnIsNarratorSpeaking += IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterHaptic += ActivateTeleporterHaptic;

        EventsManager.OnChangeHealthHaptic += ChangeHealthHaptic;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedVRPlayer -= CreatedVRPlayer;
        EventsManager.OnPingVRController -= PingVRController;
        EventsManager.OnUseEnemyHaptic -= UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic -= UseBridgeHaptic;
        EventsManager.OnUseRopeHaptic -= UseRopeHaptic;

        EventsManager.OnUseTeleportBarHaptic -= UseTeleportHaptic;
        EventsManager.OnUseButtonHaptic -= UseButtonHaptic;

        EventsManager.OnIsNarratorSpeaking -= IsNarratorSpeaking;
        EventsManager.OnActivateTeleporterHaptic -= ActivateTeleporterHaptic;

        EventsManager.OnChangeHealthHaptic -= ChangeHealthHaptic;
    }

    private void ChangeHealthHaptic(float _newHealth)
    {
        _hapticHealth = ((_newHealth / 100f));
        float _healthDelay = Mathf.Lerp(
                                            0.5f,
                                            2f,
                                            _hapticHealth
                                        );
        Debug.Log("1 _hapticHealth: " + _hapticHealth);
        if(!_playingHealthHaptic) StartCoroutine(PlayHealthHaptic(_healthDelay));
    }

    private void ActivateTeleporterHaptic()
    {
        Debug.Log("ActivateTeleporterHaptic");
        _useTeleporterHaptic = true;
        StartCoroutine(PlayTeleportHaptic(2f));
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

    private void UseBridgeHaptic(float _bridgeMovementAmount)
    {
        if(_bridgeMovementAmount < 0) _bridgeMovementAmount *= -1;
        _intensity = Mathf.Clamp(_bridgeMovementAmount * 30, 0, 1);

        bridgeHaptic.Stop();
        bridgeHaptic.SourceIntensity = _intensity;        
        bridgeHaptic.PlayEventVibration();
        Debug.Log("Bridge Haptic");
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

    private void UseButtonHaptic(int _buttonNum)
    {
        leftButtonHapticSource.Stop();
        rightButtonHapticSource.Stop();

        switch(_buttonNum)
        {
            case 0:
                _intensity = 0.25f;

                leftButtonHapticSource.SourceIntensity = _intensity;
                leftButtonHapticSource.PlayEventVibration();
                

                Debug.Log("Button 0 Left Haptic");

                break;
            case 1:
                _intensity = 1f;

                leftButtonHapticSource.SourceIntensity = _intensity;
                leftButtonHapticSource.PlayEventVibration();

                Debug.Log("Button 1 Left Haptic");

                break;
            case 2:
                _intensity = 1.75f;

                leftButtonHapticSource.SourceIntensity = _intensity;
                leftButtonHapticSource.PlayEventVibration();

                rightButtonHapticSource.SourceIntensity = _intensity;
                rightButtonHapticSource.PlayEventVibration();

                Debug.Log("Button 2 Left Right Haptic");

                break;
            case 3:
                _intensity = 1f;

                rightButtonHapticSource.SourceIntensity = _intensity;
                rightButtonHapticSource.PlayEventVibration();

                Debug.Log("Button 3 Right Haptic");

                break;
            case 4:
                _intensity = 0.25f;

                rightButtonHapticSource.SourceIntensity = _intensity;
                rightButtonHapticSource.PlayEventVibration();

                Debug.Log("Button 4 Right Haptic");

                break;
        }
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

    private IEnumerator PlayEnemyHaptic(float _delay)
    {
        _intensity = 1f - Mathf.Clamp(Vector3.Distance(leftHeadListener.position, narratorLeftHeadTransform.position) * (1f / 20f), 0, 1);
        narratorLeftHeadHaptic.SourceIntensity = _intensity;

        _intensity = 1f - Mathf.Clamp(Vector3.Distance(rightHeadListener.position, narratorRightHeadTransform.position) * (1f / 20f), 0, 1);
        narratorRightHeadHaptic.SourceIntensity = _intensity;
        
        narratorLeftHeadHaptic.PlayEventVibration();
        narratorRightHeadHaptic.PlayEventVibration();
        Debug.Log("Enemy Haptic");

        yield return new WaitForSeconds(_delay);

        // if(_isUsingEnemyHaptic) StartCoroutine(PlayEnemyHaptic(_delay));
    }

    private IEnumerator PlayTeleportHaptic(float _delay)
    {
       Debug.Log("_teleportIntensity: " + _teleportIntensity);

        teleporterHapticSource.Stop();
        teleporterHapticSource.SourceIntensity = _teleportIntensity;
        teleporterHapticSource.PlayEventVibration();
        Debug.Log("Teleport Haptic");

        yield return new WaitForSeconds(_delay);

        if(_useTeleporterHaptic) StartCoroutine(PlayTeleportHaptic(_delay));
    }

    private IEnumerator PlayHealthHaptic(float _delay)
    {
        _playingHealthHaptic = true;

        pcDamageHapticSource.Stop();
        pcDamageHapticSource.SourceIntensity = (1f - _hapticHealth) * 2;
        pcDamageHapticSource.PlayEventVibration();

        yield return new WaitForSeconds(_delay);

        if(_hapticHealth < 1f)
        {
            _hapticHealth = _hapticHealth + 0.05f;
            float _healthDelay = Mathf.Lerp(0.5f, 2f, _hapticHealth);

            Debug.Log("2 _hapticHealth: " + _hapticHealth);
            StartCoroutine(PlayHealthHaptic(_healthDelay));
        }
        else
        {
            _playingHealthHaptic = false;
        }
    }
}