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

    public Transform leftController;
    private HapticImpulsePlayer leftControllerHaptic;
    public Transform rightController;
    private HapticImpulsePlayer rightControllerHaptic;




    private bool _canUseHaptics;
    private bool _isUsingEnemyHaptic;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedVRPlayer -= CreatedVRPlayer;
        EventsManager.OnPingVRController -= PingVRController;
        EventsManager.OnUseEnemyHaptic -= UseEnemyHaptic;
        EventsManager.OnUseBridgeHaptic -= UseBridgeHaptic;
        EventsManager.OnUseRopeHaptic -= UseRopeHaptic;
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
        if(_isUsingEnemyHaptic != _useHaptic)
        {
            _isUsingEnemyHaptic = _useHaptic;
            if(_useHaptic) StartCoroutine(PlayEnemyHaptic(2f));
        }
    }

    private void UseBridgeHaptic(float _bridgeMovementAmount)
    {
        if(_bridgeMovementAmount < 0) _bridgeMovementAmount *= -1;
        _intensity = Mathf.Clamp(_bridgeMovementAmount * 30, 0, 1);

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

    private void Update()
    {
        if(!_canUseHaptics) return;

        // _intensity = 1f - Mathf.Clamp(Vector3.Distance(listenPoint.position, leftController.position) * (1f / 6f), 0, 1);
        // leftControllerHaptic.SendHapticImpulse(_intensity, 0.1f);

        // _intensity = 1f - Mathf.Clamp(Vector3.Distance(listenPoint.position, rightController.position) * (1f / 6f), 0, 1);
        // rightControllerHaptic.SendHapticImpulse(_intensity, 0.1f);
    }

    private IEnumerator PlayEnemyHaptic(float _delay)
    {
        _intensity = 1f - Mathf.Clamp(Vector3.Distance(leftHeadListener.position, narratorLeftHeadTransform.position) * (1f / 20f), 0, 1);
        narratorLeftHeadHaptic.SourceIntensity = _intensity;

        _intensity = 1f - Mathf.Clamp(Vector3.Distance(rightHeadListener.position, narratorRightHeadTransform.position) * (1f / 20f), 0, 1);
        narratorRightHeadHaptic.SourceIntensity = _intensity;
        
        narratorLeftHeadHaptic.PlayEventVibration();
        narratorRightHeadHaptic.PlayEventVibration();

        yield return new WaitForSeconds(_delay);

        if(_isUsingEnemyHaptic) StartCoroutine(PlayEnemyHaptic(_delay));
    }
}