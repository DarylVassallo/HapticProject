using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering.Universal;

public class RenderManager : MonoBehaviour
{
    public UniversalRenderPipelineAsset urpAsset;

    private float smoothedDeltaTime;
    private float timePassed;
    [SerializeField] private float timeBetweenChecks = 0.5f;

    private float currentFPS;
    // private float currentRenderScale;

    // [SerializeField] private float minRenderScale;
    // [SerializeField] private float maxRenderScale;
    // [SerializeField] private float renderScaleIncrement;

    [SerializeField] private float fpsMinLimit;
    private float fpsMinLimitCount;

    [SerializeField] private float fpsMaxLimit;
    private float fpsMaxLimitCount;

    [SerializeField] private float fpsLimitCount;

    void Awake()
    {
        XRSettings.eyeTextureResolutionScale = 1f;
        urpAsset.renderScale = 1f;

        fpsMinLimitCount = fpsLimitCount;
        fpsMaxLimitCount = fpsLimitCount;
    }

    // Used ChatGPT to form FPS calculation and FPS adjustment
    void Update()
    {
        smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
        timePassed += Time.unscaledDeltaTime;

        if(timePassed < timeBetweenChecks) return;

        currentFPS = 1f / smoothedDeltaTime;
        if(currentFPS < fpsMinLimit)
        {
            fpsMinLimitCount--;
        }
        else
        {
            fpsMinLimitCount = fpsLimitCount;
        }

        if(fpsMinLimitCount == 0)
        {
            fpsMinLimitCount = fpsLimitCount;

            if(urpAsset.renderScale > 0.1)
            {
                XRSettings.eyeTextureResolutionScale = XRSettings.eyeTextureResolutionScale - 0.05f;
                urpAsset.renderScale = urpAsset.renderScale - 0.05f;
            }
        }

        if(currentFPS > fpsMaxLimit)
        {
            fpsMaxLimitCount--;
        }
        else
        {
            fpsMaxLimitCount = fpsLimitCount;
        }

        if(fpsMaxLimitCount == 0)
        {
            fpsMaxLimitCount = fpsLimitCount;

            if(urpAsset.renderScale < 1)
            {
                XRSettings.eyeTextureResolutionScale = XRSettings.eyeTextureResolutionScale + 0.05f;
                urpAsset.renderScale = urpAsset.renderScale + 0.05f;
            }
        }

        timePassed = 0;
    }
}