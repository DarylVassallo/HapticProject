using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

using Interhaptics;
using Interhaptics.Utils;

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

    [Header("Materials")]
    [SerializeField] private Material irisMaterial;
    [SerializeField] private Material irisOffMaterial;
    [SerializeField] private Material silentMaterial;
    [SerializeField] private Material loudMaterial;
    [SerializeField] private Material visionMaterial;

    [SerializeField] private Transform _moveTarget;
    [SerializeField] private Transform _angleTarget;
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

    [Header("Specific View Points")]
    [SerializeField] private Transform _roofViewPoint;
    [SerializeField] private Transform _tutorialExitViewPoint;

    private float _prevDistance;
    private float _distance;
    private bool _canCollide;

    private ParticleSystem _particle;

    private bool _isPaused;
    private bool _canCheckVelocity;

    private bool _isInNetwork;

    private void Awake()
    {
        _canCheckVelocity = true;
        _isPaused = false;

        _particle = this.GetComponent<ParticleSystem>();

        Color colour = visionMaterial.color;
        colour.a = 0;
        visionMaterial.color = colour;  

        _canCollide = true;
        this.GetComponent<Collider>().enabled = true;
        _particle.Stop();

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
        EventsManager.OnToggleAll += TogglePause;

        EventsManager.OnCreatedPCPlayerBody += GetPCPlayerBodyDataRpc;
        EventsManager.OnCreatedVRPlayer += GetVRPlayerData; 

        EventsManager.OnLookAtPlayer += LookAtPlayerRpc;

        EventsManager.OnNarratorSays += NarratorSays;
        EventsManager.OnTogglePauseManagerAudio += TogglePauseManagerAudioRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnToggleAll -= TogglePause;

        EventsManager.OnCreatedPCPlayerBody -= GetPCPlayerBodyDataRpc;
        EventsManager.OnCreatedVRPlayer -= GetVRPlayerData;

        EventsManager.OnLookAtPlayer -= LookAtPlayerRpc; 

        EventsManager.OnNarratorSays -= NarratorSays;
        EventsManager.OnTogglePauseManagerAudio -= TogglePauseManagerAudioRpc;
    }

    public override void OnNetworkSpawn()
    {
        _isInNetwork = true;
    }

    private void TogglePause(bool _toggle)
    {
        _isPaused = !_toggle;

        if(_toggle)
        { 
            _rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(CheckVelocityDelay(1f));
        }
        else
        {
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            _particle.Stop();
            _canCheckVelocity = false;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetPCPlayerBodyDataRpc()
    {
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
    private void LookAtPlayerRpc(int _playerNum, int _specificViewPoint, bool _stay)
    {
        bool _showVision = false;

        switch(_playerNum)
        {
            case -1:
                _lookAround = true;
                StartCoroutine(ChangeFocus(UnityEngine.Random.Range(minFocusTime, maxFocusTime)));
                break;
            case 0:
                // this.GetComponent<Collider>().enabled = false;
                if(_vrViewPoint != null)
                {
                    _moveTarget = _vrViewPoint;
                    _angleTarget = _vrViewPoint;
                }

                _lookAround = false;
                _showVision = false;

                if(_moveTarget != null)
                {
                    _prevDistance = Vector3.Distance(_moveTarget.position, this.transform.position);
                }
                break;
            case 1:
                // this.GetComponent<Collider>().enabled = false;
                if(_pcViewPoint != null)
                {
                    _moveTarget = _pcViewPoint;
                    _angleTarget = _pcViewPoint;
                }

                _lookAround = false;
                _showVision = false;

                if(_moveTarget != null)
                {
                    _prevDistance = Vector3.Distance(_moveTarget.position, this.transform.position);
                }
                break;
        }

        if(_moveTarget != null)
        {
            _distance = Vector3.Distance(_moveTarget.position, this.transform.position);
            if(_distance >= 50f)
            {
                if(IsOwner) DisableCollisionServerRpc();
            }
        }
        
        switch(_specificViewPoint)
        {
            case 0:
                _angleTarget = _roofViewPoint;
                _showVision = true;
                break;
            case 1:
                _angleTarget = _tutorialExitViewPoint;
                _showVision = true;
                break;
        }

        if(_showVision)
        {
            StartCoroutine(ChangeVision(1f, 1f));
        }
        else
        {
            StartCoroutine(ChangeVision(1f, 0f));
        }

        _stayWithPlayer = _stay;

        StartCoroutine(CheckVelocityDelay(1f));
    }

    IEnumerator ChangeVision(float _delay, float _requiredAlpha)
    {
        if(visionMaterial.color.a != _requiredAlpha)
        {
            float elapsed = 0f;
            while(elapsed < _delay)
            {
                elapsed += Time.deltaTime;
                
                Color colour = visionMaterial.color;
                colour.a = _requiredAlpha * (elapsed / _delay);
                visionMaterial.color = colour;           

                yield return null;
            }
        }
    }

    IEnumerator ChangeFocus(float _delay)
    {
        this.GetComponent<Collider>().enabled = true;
        _particle.Stop();

        bool _foundTarget = false;
        for(int i = 0; i < 50; i++)
        {
            _moveTarget = _narratorViewPoints[UnityEngine.Random.Range(0, _narratorViewPoints.Count - 1)];

            if(!Physics.Linecast(this.transform.position, _moveTarget.position, out RaycastHit hit, obstacleLayer))
            {
                _prevDistance = Vector3.Distance(_moveTarget.position, this.transform.position);
                _foundTarget = true;
                break;
            }
        }

        if(!_foundTarget)
        {
            _moveTarget = _narratorViewPoints[UnityEngine.Random.Range(0, _narratorViewPoints.Count - 1)];
            _prevDistance = Vector3.Distance(_moveTarget.position, this.transform.position);
            _foundTarget = true;
        }

        StartCoroutine(CheckVelocityDelay(1f));

        yield return new WaitForSeconds(_delay);

        if(_lookAround) StartCoroutine(ChangeFocus(UnityEngine.Random.Range(minFocusTime, maxFocusTime)));
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TogglePauseManagerAudioRpc(bool _toggle)
    {
        if(!_toggle)
        {
            _speakToPlayer = true;
        }
    }

    private void NarratorSays(AudioClip _audioClip, AudioHapticSource _newHapticSource)
    {
        _isPlaying = false;

        _audioSource.Stop();
        _audioSource.clip = _audioClip;
        _audioSource.pitch = 1f;

        EventsManager.ChangeNarratorHaptic(_newHapticSource);

        _speakToPlayer = true;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerNarratorAudioRpc()
    {        
        // _audioSource.Play();

        EventsManager.PlayNarratorHaptic();
        EventsManager.IsNarratorSpeaking(true);

        _isPlaying = true;
        _audioSource.enabled = true; 
        _speakToPlayer = false;
    }

    private void FixedUpdate()
    {        
        if(_isPaused) return;

        if(_canFlicker && _isInNetwork) IrisFlickerServerRpc();

        if(_moveTarget != null) Movement();
        RotateRings();
        AudioEyeRing();

        if(!_isPlaying)
        {
            if(_moveTarget != null)
            {
                _angleTarget = _moveTarget;
                StartCoroutine(ChangeVision(1f, 0f));
            }
            return;
        }

        if(_isPlaying && !_audioSource.isPlaying && !_isPaused)
        {
            _isPlaying = false;

            EventsManager.StopNarratorHaptic();
            EventsManager.IsNarratorSpeaking(false);

            EventsManager.NarratorStopped();

            if(!_stayWithPlayer)
            {
                LookAtPlayerRpc(-1, -1, false);
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void IrisFlickerServerRpc()
    {
        IrisFlickerRpc(UnityEngine.Random.Range(0.01f, 0.5f), UnityEngine.Random.Range(0f, 1f));
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void IrisFlickerRpc(float _delay, float _flicker)
    {
        StartCoroutine(IrisFlicker(_delay, _flicker));
    }

    IEnumerator IrisFlicker(float _delay, float _flicker)
    {
        _canFlicker = false;
        if(_delay >= 0.05f) _flicker = 1;

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
        if(_angleTarget == null) return;
        
        Vector3 direction = _angleTarget.position - transform.position;

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

        _distance = Vector3.Distance(_moveTarget.position, this.transform.position);
        float _force = 1f;

        if(!_canCollide)
        {
            Vector3 direction = _moveTarget.position - transform.position;
            _rb.AddForce(direction * _force * 10, ForceMode.Force);
            _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f * 10f);
        }else{
            if(_distance >= 10f)
            {
                Vector3 direction = _moveTarget.position - transform.position;

                if(_canCollide)
                {
                    _rb.AddForce(direction * _force, ForceMode.Force);
                    _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f);
                }

                if((_rb.linearVelocity.sqrMagnitude < 0.001f) && _canCollide && _canCheckVelocity)
                {
                    if(IsOwner) DisableCollisionServerRpc();
                }
            }
            else if(_distance < 6f)
            {
                Vector3 direction = transform.position - _moveTarget.position;
                _rb.AddForce(direction * _force, ForceMode.Force);
                _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, 20f);
            }
            else
            {
                if(!this.GetComponent<Collider>().enabled)
                {
                    this.GetComponent<Collider>().enabled = true;
                    _particle.Stop();
                }
                
                if(_speakToPlayer) PlayerNarratorAudioRpc();
            }
        }

        _prevDistance = _distance;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DisableCollisionServerRpc()
    {
        StartCoroutine(DisableCollision());
    }
    
    private IEnumerator DisableCollision()
    {
        if(_moveTarget != null)
        {
            _distance = Vector3.Distance(_moveTarget.position, this.transform.position);
            float _originalDistance = _distance;
            Debug.Log("_originalDistance: " + _originalDistance);

            _canCollide = false;
            this.GetComponent<Collider>().enabled = false;
            _particle.Play();

            // yield return new WaitForSeconds(_delay);
            while(_distance > _originalDistance * 0.5f)
            {      
                _distance = Vector3.Distance(_moveTarget.position, this.transform.position);
                Debug.Log("curr _distance: " + _distance);
                yield return null;
            }
        }

        _canCollide = true;
        this.GetComponent<Collider>().enabled = true;
        _particle.Stop();
    }

    private IEnumerator CheckVelocityDelay(float _delay)
    {
        _canCheckVelocity = false;

        yield return new WaitForSeconds(_delay);

        _canCheckVelocity = true;
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
