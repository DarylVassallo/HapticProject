using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering.Universal;

public class RenderManager : MonoBehaviour
{
    [SerializeField] private int targetFPS;
    public UniversalRenderPipelineAsset urpAsset;

    private float smoothedDeltaTime;
    private float timePassed;
    [SerializeField] private float timeBetweenChecks = 0.5f;

    private float currentFPS;
    private float currentRenderScale;

    [SerializeField] private float minRenderScale;
    [SerializeField] private float maxRenderScale;
    [SerializeField] private float renderScaleIncrement;

    //Used ChatGPT to form FPS calculation and FPS adjustment

    void Update()
    {
        smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * 0.1f;

        // Debug.Log("smoothedDeltaTime: " + smoothedDeltaTime);

        timePassed += Time.unscaledDeltaTime;

        if(timePassed < timeBetweenChecks) return;
        Debug.Log("=================");
        Debug.Log("old urpAsset.renderScale: " + urpAsset.renderScale);

        timePassed = 0;

        currentFPS = 1f / smoothedDeltaTime;
        currentRenderScale = urpAsset.renderScale;

        Debug.Log("currentFPS: " + currentFPS);
        Debug.Log("currentRenderScale: " + currentRenderScale);
        
        if(currentFPS < (targetFPS - 5))
        {
            Debug.Log("(currentRenderScale - renderScaleIncrement): " + (currentRenderScale - renderScaleIncrement));
            currentFPS = Mathf.Max(minRenderScale, currentRenderScale - renderScaleIncrement);
        }else if(currentFPS > (targetFPS + 5))
        {
            Debug.Log("(currentRenderScale + renderScaleIncrement): " + (currentRenderScale + renderScaleIncrement));
            currentFPS = Mathf.Min(maxRenderScale, currentRenderScale + renderScaleIncrement);
        }
        
        XRSettings.eyeTextureResolutionScale = currentFPS;
        urpAsset.renderScale = currentFPS;
        Debug.Log("new urpAsset.renderScale: " + urpAsset.renderScale);
    }
}
