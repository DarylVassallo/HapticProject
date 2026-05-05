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
    private bool _isInteractable;

    Vector3 positionDifference;
    float positionDistance;
    Vector3 positionDirection;
    Vector3 spotlightDirection;
    float scale;
    float angleRad;
    float threshold;
    float range;
    float strength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created.
    void Awake()
    {
        _hiddenMaterial = GetComponent<Renderer>().material;
        
        _pcSpotLight = GameObject.FindGameObjectWithTag("PCPlayer").GetComponentInChildren<Light>();
        _vrSpotLight = GameObject.FindGameObjectWithTag("VRPlayer").GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isEffectedByLight)
        {
            if (CheckLightStrength() >= 0.15f)
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

    private float CheckLightStrength()
    {
        positionDifference = transform.position - _vrSpotLight.transform.position;
        positionDistance = positionDifference.magnitude;
        positionDirection = positionDifference.normalized;

        spotlightDirection = _vrSpotLight.transform.forward.normalized;

        scale = Vector3.Dot(positionDirection, spotlightDirection);

        angleRad = _vrSpotLight.spotAngle * 0.5f * Mathf.Deg2Rad;
        threshold = Mathf.Cos(angleRad);

        range = Mathf.Clamp01(1.0f - (positionDistance * positionDistance) / ((_vrSpotLight.range * 0.5f) * (_vrSpotLight.range * 0.5f)));
        strength = Mathf.Clamp01((scale - threshold)) * range;

        return strength;
    }
}
