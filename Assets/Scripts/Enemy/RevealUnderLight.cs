using UnityEngine;
//This script updates the hidden object's shader to render correctly based on the spotlights position, direction, and angle (modified to include range) (modifications to the shader used chatgpt).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class RevealUnderLight : MonoBehaviour
{
    private Material _hiddenMaterial;
    private Light _spotLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created.
    void Awake()
    {
        _hiddenMaterial = GetComponent<Renderer>().material;
        _spotLight = GameObject.FindGameObjectWithTag("PCPlayer").GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_hiddenMaterial && _spotLight)
        {
            if (!_spotLight.enabled)
            {
                _hiddenMaterial.SetFloat("_MyLightRange", 0);
            }else{
                _hiddenMaterial.SetVector("_MyLightPosition", _spotLight.transform.position);
                _hiddenMaterial.SetVector("_MyLightDirection", -_spotLight.transform.forward);
                _hiddenMaterial.SetFloat("_MyLightAngle", _spotLight.spotAngle);
                _hiddenMaterial.SetFloat("_MyLightRange", _spotLight.range * 0.5f);
            }
        }
    }
}
