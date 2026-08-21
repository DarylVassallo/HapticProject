using UnityEngine;
using UnityEngine.Rendering.Universal;

using TMPro;

using System.Collections.Generic;
using UnityEngine.XR;

using System.Collections;

public class ShowFPS : MonoBehaviour
{
    private float smoothedDeltaTime;
    private float timePassed;
    [SerializeField] private float timeBetweenChecks = 0.5f;

    private TextMeshPro fpsText;

    private float currentFPS;

    void Start()
    {
        fpsText = GetComponent<TextMeshPro>();
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 0;
        XRSettings.eyeTextureResolutionScale = 0.55f;
        Application.runInBackground = true;
    }

    void Update()
    {

        smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
        timePassed += Time.unscaledDeltaTime;

        if(timePassed < timeBetweenChecks) return;

        currentFPS = 1f / smoothedDeltaTime;

        float fps = 1f / Time.unscaledDeltaTime;
        float refreshRate = XRDevice.refreshRate;
        fpsText.text = $"FPS: {fps:F1}";

        timePassed = 0;
    }
}
