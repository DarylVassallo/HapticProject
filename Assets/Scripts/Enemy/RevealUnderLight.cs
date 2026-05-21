using UnityEngine;
//This script updates the hidden object's shader to render correctly based on the spotlights position, direction, and angle (modified to include range) (modifications to the shader used chatgpt).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class RevealUnderLight : MonoBehaviour
{
    private Material _hiddenMaterial;
    private Light _pcSpotLight;
    private Light _vrSpotLight;

    [SerializeField] private bool isPCInteractable;
    [SerializeField] private bool isVRInteractable;
    [SerializeField] private bool isEffectedByLight;
    [SerializeField] private bool isReversed;
    private bool _isInteractable;

    private Vector3 _positionDifference;
    private float _positionDistance;
    private Vector3 _positionDirection;
    private Vector3 _spotlightDirection;
    private float _scale;
    private float _angleRad;
    private float _threshold;
    private float _range;
    private float _strength;
    private float _totalStrength;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    private float _strengthLimit;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created.
    void Awake()
    {
        _hiddenMaterial = GetComponent<Renderer>().material;
        _vrSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();

        if (isReversed)
        {
            _strengthLimit = 1f;
        }else
        {
            _strengthLimit = 0.15f;
        }
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;
    }

    private void GetPCPlayerData()
    {
        _pcSpotLight = GameObject.FindGameObjectWithTag("PCPlayer").GetComponentInChildren<Light>();
        _canPCFunction = true;
    }

    private void GetVRPlayerData()
    {
        // _vrSpotLight = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponentInChildren<Light>();
        _canVRFunction = true;
    }

    void FixedUpdate()
    {
        if(!_canPCFunction && !_canVRFunction) return;

        if (isEffectedByLight)
        {
            if (CheckLightStrength() >= _strengthLimit)
            {
                if(!_isInteractable)
                {
                    _isInteractable = true;
                    if (this.GetComponent<Collider>() != null) this.GetComponent<Collider>().enabled = true;
                    if (this.GetComponent<IInteractable>() != null) this.GetComponent<IInteractable>().EnableInteraction();
                }
            }
            else
            {
                if(_isInteractable)
                {
                    _isInteractable = false;
                    if (this.GetComponent<Collider>() != null) this.GetComponent<Collider>().enabled = false;
                    if (this.GetComponent<IInteractable>() != null) this.GetComponent<IInteractable>().DisableInteraction();
                }
            }
        }
        
        if(_hiddenMaterial && _pcSpotLight && _vrSpotLight)
        {
            if (!_pcSpotLight.enabled || !_vrSpotLight.enabled)
            {
                _hiddenMaterial.SetFloat("_MyLightRange", 0);
            }else{
                if(isPCInteractable)
                {
                    _hiddenMaterial.SetVector("_PCLightPosition", _pcSpotLight.transform.position);
                    _hiddenMaterial.SetVector("_PCLightDirection", -_pcSpotLight.transform.forward);
                    _hiddenMaterial.SetFloat("_PCLightAngle", _pcSpotLight.spotAngle);
                    _hiddenMaterial.SetFloat("_PCLightRange", _pcSpotLight.range * 0.5f);
                }
                else
                {
                    _hiddenMaterial.SetVector("_PCLightPosition", new Vector3(0, 0, 0));
                    _hiddenMaterial.SetVector("_PCLightDirection", new Vector3(0, 0, 0));
                    _hiddenMaterial.SetFloat("_PCLightAngle", 0f);
                    _hiddenMaterial.SetFloat("_PCLightRange", 0f);
                }

                if(isVRInteractable)
                {
                    _hiddenMaterial.SetVector("_VRLightPosition", _vrSpotLight.transform.position);
                    _hiddenMaterial.SetVector("_VRLightDirection", -_vrSpotLight.transform.forward);
                    _hiddenMaterial.SetFloat("_VRLightAngle", _vrSpotLight.spotAngle);
                    _hiddenMaterial.SetFloat("_VRLightRange", _vrSpotLight.range * 0.5f);
                }
                else
                {
                    _hiddenMaterial.SetVector("_VRLightPosition", new Vector3(0, 0, 0));
                    _hiddenMaterial.SetVector("_VRLightDirection", new Vector3(0, 0, 0));
                    _hiddenMaterial.SetFloat("_VRLightAngle", 0f);
                    _hiddenMaterial.SetFloat("_VRLightRange", 0f);
                }
            }
        }
    }

    private float CheckPlayerLightStrength(Light _spotLight)
    {
        _positionDifference = transform.position - _spotLight.transform.position;
        _positionDistance = _positionDifference.magnitude;
        _positionDirection = _positionDifference.normalized;

        _spotlightDirection = _spotLight.transform.forward.normalized;

        _scale = Vector3.Dot(_positionDirection, _spotlightDirection);

        _angleRad = _spotLight.spotAngle * 0.5f * Mathf.Deg2Rad;
        _threshold = Mathf.Cos(_angleRad);

        _range = Mathf.Clamp01(1.0f - (_positionDistance * _positionDistance) / ((_spotLight.range * 0.5f) * (_spotLight.range * 0.5f)));
        _strength = Mathf.Clamp01((_scale - _threshold)) * _range;

        return _strength;
    }

    private float CheckLightStrength()
    {
        _totalStrength = 0;

        if(isPCInteractable) _totalStrength += CheckPlayerLightStrength(_pcSpotLight);
        if(isVRInteractable) _totalStrength += CheckPlayerLightStrength(_vrSpotLight);
        _totalStrength = Mathf.Clamp01(_totalStrength);

        if(isReversed) _totalStrength = 1 - _totalStrength;

        return _totalStrength;
    }
}
