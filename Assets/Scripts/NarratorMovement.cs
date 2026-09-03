using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class NarratorMovement : NetworkBehaviour
{
    private AudioSource _audioSource;
    private Rigidbody _rb;

    private bool _isPlaying;

    [SerializeField] private Transform floatingRing;
    [SerializeField] private Transform otherFloatingRing;

    [SerializeField] private Renderer irisRenderer;
    [SerializeField] private Renderer eyeRingRenderer;

    private bool _canFlicker;
    [SerializeField] private Material irisMaterial;
    [SerializeField] private Material irisOffMaterial;
    [SerializeField] private Material silentMaterial;
    [SerializeField] private Material loudMaterial;

    [SerializeField] private Transform _eyeTarget;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private Transform narratorViewPoint;
    private List<Transform> _narratorViewPoints;
    private Transform _pcViewPoint;
    private Transform _vrViewPoint;
    private bool _stayWithPlayer;
    private bool _lookAround;

    [SerializeField] private int minFocusTime;
    [SerializeField] private int maxFocusTime;

    private float _elapsed;

    private float minDbValue = 0.0001f; // small reference to avoid log(0), tweak to taste
    private float minDb = -160f;

    private bool _speakToPlayer;

    private void Awake()
    {
        _canFlicker = true;

        _speakToPlayer = false;
        _lookAround = false;

        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();

        _elapsed = 0;

        _narratorViewPoints = new List<Transform>();
        for (int i = 0; i < narratorViewPoint.childCount; i++)
        {
            _narratorViewPoints.Add(narratorViewPoint.GetChild(i));
        }
    }

    private void OnEnable()
    {
        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData; 

        EventsManager.OnLookAtPlayer += LookAtPlayerRpc;

        EventsManager.OnNarratorSays += NarratorSays;
        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;
        EventsManager.OnCreatedVRPlayer -= GetVRPlayerData;

        EventsManager.OnLookAtPlayer -= LookAtPlayerRpc; 

        EventsManager.OnNarratorSays -= NarratorSays;
        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetPCPlayerBodyDataRpc()
    {
        Debug.Log("GetPCPlayerBodyDataRpc");
        _pcViewPoint = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        _narratorViewPoints.Add(_pcViewPoint);
    }

    private void GetVRPlayerData()
    {
        foreach (Transform child in GameObject.FindGameObjectWithTag("VRPlayer").GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("VRCamera"))
            {
                _vrViewPoint = child;
                _narratorViewPoints.Add(_vrViewPoint);
                break;
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void LookAtPlayerRpc(int _playerNum, bool _stay)
    {
        Debug.Log("LookAtPlayerRpc: " + _playerNum + ", " + _stay);

        switch(_playerNum)
        {
            case -1:
                _lookAround = true;
                StartCoroutine(ChangeFocus(UnityEngine.Random.Range(minFocusTime, maxFocusTime)));
                break;
            case 0:
                this.GetComponent<Collider>().enabled = false;
                _eyeTarget = _vrViewPoint;
                _lookAround = false;
                break;
            case 1:
                this.GetComponent<Collider>().enabled = false;
                Debug.Log("_pcViewPoint: " + _pcViewPoint);
                _eyeTarget = _pcViewPoint;
                _lookAround = false;
                break;
        }

        _stayWithPlayer = _stay;
    }

    IEnumerator ChangeFocus(float delay)
    {
        this.GetComponent<Collider>().enabled = true;
        bool _foundTarget = false;
        do
        {
            _eyeTarget = _narratorViewPoints[UnityEngine.Random.Range(0, _narratorViewPoints.Count - 1)];

            if(!Physics.Linecast(this.transform.position, _eyeTarget.position, out RaycastHit hit, obstacleLayer))
            {
                _foundTarget = true;
            }
        }while(!_foundTarget);

        yield return new WaitForSeconds(delay);

        if(_lookAround) StartCoroutine(ChangeFocus(UnityEngine.Random.Range(minFocusTime, maxFocusTime)));
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TogglePauseManagerAudioRpc(bool _toggle)
    {
        if(_toggle)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }

    private void NarratorSays(AudioClip _clip)
    {
        Debug.Log("NarratorSays: " + _clip);

        _audioSource.Stop();
        _audioSource.clip = _clip;
        _audioSource.pitch = 1f;

        _speakToPlayer = true;
    }

    private void PlayerNarratorAudio()
    {
        Debug.Log("PlayerNarratorAudio");

        _audioSource.Play();
        _isPlaying = true;
        _audioSource.enabled = true; 

        _speakToPlayer = false;
    }

    private void FixedUpdate()
    {        
        if(_canFlicker) IrisFlickerServerRpc();

        Movement();
        RotateRings();
        AudioEyeRing();

        if(!_isPlaying) return;

        if(!_audioSource.isPlaying)
        {
            _isPlaying = false;
            EventsManager.NarratorStopped();

            if(!_stayWithPlayer) LookAtPlayerRpc(-1, false);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void IrisFlickerServerRpc()
    {
        StartCoroutine(IrisFlicker(UnityEngine.Random.Range(0.01f, 0.5f), UnityEngine.Random.Range(0f, 1f)));
    }

    IEnumerator IrisFlicker(float _delay, float _flicker)
    {
        _canFlicker = false;
        if(_delay >= 0.2f) _flicker = 1;

        yield return new WaitForSeconds(_delay);

        irisRenderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    irisOffMaterial.GetColor("_BaseColor"),
                    irisMaterial.GetColor("_BaseColor"),
                    _flicker
                )
            );

        _canFlicker = true;
    }

    private void RotationControl()
    {
        Vector3 direction = _eyeTarget.position - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 axis);

        Vector3 torque = axis * (angleInDegrees * Mathf.Deg2Rad) * 50f;
        Vector3 dampingTorque = -_rb.angularVelocity * 10f;

        _rb.AddTorque(torque + dampingTorque, ForceMode.Acceleration);
    }

    private void Movement()
    {
        RotationControl();

        _elapsed += Time.deltaTime;
        _rb.AddForce(Vector3.up * Mathf.Sin(_elapsed) / 2f, ForceMode.Force);

        float _distance = Vector3.Distance(_eyeTarget.position, this.transform.position);
        float _force = 1f;

        if(_distance >= 10f)
        {
            Vector3 direction = _eyeTarget.position - transform.position;
            _rb.AddForce(direction * _force, ForceMode.Force);
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f);
        }
        else if(_distance < 6f)
        {
            Vector3 direction = transform.position - _eyeTarget.position;
            _rb.AddForce(direction * _force, ForceMode.Force);
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f);
        }
        else
        {
            if(!this.GetComponent<Collider>().enabled) this.GetComponent<Collider>().enabled = true;
            
            if(_speakToPlayer)
            {
                Debug.Log("Speak");
                PlayerNarratorAudio();
            }
        }
    }

    private void RotateRings()
    {
        floatingRing.Rotate(Vector3.forward * 100f * Time.deltaTime);
        otherFloatingRing.Rotate(Vector3.forward * -100f * Time.deltaTime);
    }

    private float GetDecibel(float[] spectrum, float length)
    {
        float value = -1;
        float maxValue = 0;
        for(int i = 0; i < length; i++)
        {
            value = 20f * Mathf.Log10(Mathf.Max(spectrum[i], minDbValue) / minDbValue);
            maxValue += value;
        }

        return Mathf.Max(maxValue / length, minDb);
    }

    private void AudioEyeRing()
    {
        float[] spectrum = new float[256];
        _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        float decibel = GetDecibel(spectrum, 16f);

        eyeRingRenderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    silentMaterial.GetColor("_BaseColor"),
                    loudMaterial.GetColor("_BaseColor"),
                    decibel / 60f
                )
            );
    }
}
