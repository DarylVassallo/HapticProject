using UnityEngine;
using System.Collections.Generic;
using System;
//This controls the various things that could occur due to the hidden switches
public class MazeManager : MonoBehaviour
{
    private bool _isRotatingFirstBridges;
    private bool _isMovingFirstBridges;
    [SerializeField] private Transform firstBridges;

    private bool _isRotatingSecondBridges;
    [SerializeField] private Transform secondBridges;

    private bool _isRotatingThirdBridges;
    [SerializeField] private Transform thirdBridges;

    [SerializeField] private float bridgeRotateSpeed;


    [SerializeField] private GameObject angel;
    [SerializeField] private Transform angelSpawnPoints;
    [SerializeField] private float spawnTooFarRange;
    [SerializeField] private float spawnTooCloseRange;
    private Transform _pcPlayer;

    private List<Transform> _closeSpawnPoints;

    public enum ShapeType
    {
        Triangle,
        Square,
        Circle,
        Pentagon,
        Diamond
    }

    [Header("Triangle")]
    [SerializeField] private float triangleChancesOfAngel;
    [SerializeField] private int triangleMaxAngels;
    private bool _isTriangleActive;

    [Header("Square")]
    [SerializeField] private float squareChancesOfAngel;
    [SerializeField] private int squareMaxAngels;
    private bool _isSquareActive;

    [Header("Circle")]
    [SerializeField] private float circleChancesOfAngel;
    [SerializeField] private int circleMaxAngels;
    private bool _isCircleActive;

    private float lerpTargetY;

    public static event Action OnDrainFlashlight;
    
    void Awake()
    {        
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        _closeSpawnPoints = new List<Transform>();
    }

    private void OnEnable()
    {
        ButtonInteract.OnTriggerButton += Activate;
    }

    private void OnDisable()
    {
        ButtonInteract.OnTriggerButton -= Activate;
    }

    void FixedUpdate()
    {
        if (_isMovingFirstBridges)
        {
            Debug.Log("firstBridges.position.y: " + firstBridges.position.y);
            Vector3 targetPosition = new Vector3(
                firstBridges.position.x,
                lerpTargetY,
                firstBridges.position.z
            );

            firstBridges.position = Vector3.Lerp(
                firstBridges.position,
                targetPosition,
                Time.deltaTime * 2f // speed factor
            );

            if (Mathf.Abs(firstBridges.position.y - lerpTargetY) <= 0.05)
            {
                firstBridges.position = new Vector3(firstBridges.position.x, lerpTargetY, firstBridges.position.z);
                _isMovingFirstBridges = !_isMovingFirstBridges;
            }
        }

        if (_isRotatingFirstBridges) firstBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);

        if (_isRotatingSecondBridges) secondBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);

        if (_isRotatingThirdBridges) thirdBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);
    }

    private void Activate(ShapeType shape)
    {
        switch(shape)
        {
            case ShapeType.Triangle:
                _isTriangleActive = !_isTriangleActive;
                ToggleFirstBridgesPosition();
                AngelCreation(triangleChancesOfAngel, triangleMaxAngels);
                break;

            case ShapeType.Square:
                _isSquareActive = !_isSquareActive;
                AngelCreation(squareChancesOfAngel, squareMaxAngels);
                break;

            case ShapeType.Circle:
                _isCircleActive = !_isCircleActive;
                DrainFlashlightCharge();
                AngelCreation(circleChancesOfAngel, circleMaxAngels);
                break;
        }
    }

    private void AngelCreation(float chancesOfAngel, int maxAngels)
    {
        float _chanceOfAngel = UnityEngine.Random.Range(0f, 1f);

        if (_chanceOfAngel <= chancesOfAngel)
        {
            int _numberOfAngels = UnityEngine.Random.Range(1, maxAngels);

            for (int i = 0; i < _numberOfAngels; i++)
            {
                InstantiateRandomAngel();
            }
        }
    }

    private void DrainFlashlightCharge()
    {
        OnDrainFlashlight?.Invoke();
    }

    private void ToggleFirstBridgesPosition()
    {
        _isMovingFirstBridges = !_isMovingFirstBridges;

        lerpTargetY = -100;
        if (firstBridges.position.y == 0f)
        {
            lerpTargetY = -20f;
        }else if (firstBridges.position.y == -20f)
        {
            lerpTargetY = 0f;
        }
    }

    private void ToggleFirstBridgesRotation()
    {
        _isRotatingFirstBridges = !_isRotatingFirstBridges;
    }

    private void ToggleSecondBridges()
    {
        _isRotatingSecondBridges = !_isRotatingSecondBridges;
    }

    private void ToggleThirdBridges()
    {
        _isRotatingThirdBridges = !_isRotatingThirdBridges;
    }

    private void InstantiateRandomAngel()
    {
        float distance = 0;
        for (int i = 0; i < angelSpawnPoints.childCount; i++)
        {
            distance = (angelSpawnPoints.GetChild(i).position - _pcPlayer.position).magnitude;

            if (distance > spawnTooCloseRange && distance <= spawnTooFarRange)
            {
                _closeSpawnPoints.Add(angelSpawnPoints.GetChild(i));
            }
        }

        int angelNum = UnityEngine.Random.Range(1, _closeSpawnPoints.Count) - 1;
        Instantiate(angel, _closeSpawnPoints[angelNum].position, Quaternion.identity);
    }
}
