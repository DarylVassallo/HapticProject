using UnityEngine;

public class RevealUnderLight : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] Light spotLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(mat && spotLight)
        {
            mat.SetVector("_MyLightPosition", spotLight.transform.position);
            mat.SetVector("_MyLightDirection", -spotLight.transform.forward);
            mat.SetFloat("_MyLightAngle", spotLight.spotAngle);
        }
    }
}
