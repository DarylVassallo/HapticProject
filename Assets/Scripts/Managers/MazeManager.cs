using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VRTemplate;

using UnityEngine.Events;

//This controls the various things that could occur due to the hidden switches
public class MazeManager : MonoBehaviour
{
    [SerializeField] private Transform signs;

    private bool _isMovingObject;
    private Transform movingObject;

    [Header("Bridges")]
    [SerializeField] private Transform firstBridges;
    private bool _isRotatingFirstBridges;

    [SerializeField] private Transform secondBridges;
    private bool _isRotatingSecondBridges;

    [SerializeField] private Transform thirdBridges;
    private bool _isRotatingThirdBridges;

    [SerializeField] private float bridgeRotateSpeed;

    [Header("Angel")]
    [SerializeField] private GameObject angel;
    [SerializeField] private Transform angelSpawnPoints;
    [SerializeField] private float spawnTooFarRange;
    [SerializeField] private float spawnTooCloseRange;
    private Transform _pcPlayer;

    private List<Transform> _closeSpawnPoints;

    [System.Serializable]
    public struct HiddenSwitches
    {
        public ShapeType shape;
        public ButtonType[] buttonOrder;
        public UnityEvent activateMethod;
    }
    public HiddenSwitches[] hiddenSwitches;

    public enum ShapeType
    {
        None,
        Triangle,
        Square,
        Circle,
        Pentagon,
        Diamond
    }

    public enum ButtonType
    {
        None,
        Square,
        Circle,
        Triangle,
        Cross,
        Star
    }

    private ShapeType[] currentShapeOrder = new ShapeType[5];
    private ButtonType[] currentButtonOrder = new ButtonType[5];
    private int entryNum = 0;


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

    [Header("Pentagon")]
    [SerializeField] private float pentagonChancesOfAngel;
    [SerializeField] private int pentagonMaxAngels;
    private bool _isPentagonActive;

    [Header("Diamond")]
    [SerializeField] private float diamondChancesOfAngel;
    [SerializeField] private int diamondMaxAngels;
    private bool _isDiamondActive;

    private float lerpTargetY;

    public static event Action OnDrainFlashlight;

    public XRKnob knob;
    private float prevKnobValue = 0;
    private float knobValueDiff = 0;

    private bool canPCFunction = false;
    
    void Awake()
    {        
        _closeSpawnPoints = new List<Transform>();
    }

    private void OnEnable()
    {
        ButtonInteract.OnTriggerButton += PressedButton;

        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
    }

    private void OnDisable()
    {
        ButtonInteract.OnTriggerButton -= PressedButton;

        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
    }

    private void GetPCPlayerData()
    {
        _pcPlayer = GameObject.FindGameObjectWithTag("PCPlayer").transform;
        canPCFunction = true;
    }

    void FixedUpdate()
    {
        if(!canPCFunction) return;
        
        if (_isMovingObject)
        {
            Vector3 targetPosition = new Vector3(
                movingObject.position.x,
                lerpTargetY,
                movingObject.position.z
            );

            movingObject.position = Vector3.Lerp(
                movingObject.position,
                targetPosition,
                Time.deltaTime * 2f
            );

            if (Mathf.Abs(movingObject.position.y - lerpTargetY) <= 0.05)
            {
                movingObject.position = new Vector3(movingObject.position.x, lerpTargetY, movingObject.position.z);
                _isMovingObject = !_isMovingObject;
            }
        }

        knobValueDiff = knob.value - prevKnobValue;
        firstBridges.Rotate(0.0f, knobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
        secondBridges.Rotate(0.0f, knobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
        thirdBridges.Rotate(0.0f, knobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);

        prevKnobValue = knob.value;

        // if (_isRotatingFirstBridges) firstBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);

        // if (_isRotatingSecondBridges) secondBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);

        // if (_isRotatingThirdBridges) thirdBridges.Rotate(0.0f, Time.deltaTime * bridgeRotateSpeed, 0.0f, Space.Self);
    }

    public void ActivateWheel()
    {
        
    }

    private void PressedButton(ShapeType _shape, ButtonType button)
    {
        for(int i = 0; i < currentShapeOrder.Length; i++)
        {
            if(currentShapeOrder[i] == ShapeType.None)
            {
                break;
            } 

            if(currentShapeOrder[i] != _shape)
            {
                currentShapeOrder = new ShapeType[5];
                currentButtonOrder = new ButtonType[5];
                entryNum = 0;
                break;
            }
        }
        
        currentShapeOrder[entryNum] = _shape;
        currentButtonOrder[entryNum] = button;
        entryNum++;

        for (int i = 0; i < hiddenSwitches.Length; i++)
        {
            if(hiddenSwitches[i].shape == _shape)
            {
                for (int j = 0; j < hiddenSwitches[i].buttonOrder.Length; j++)
                {
                    if(currentButtonOrder[j] == ButtonType.None)
                    {
                        return; 
                    }

                    if (hiddenSwitches[i].buttonOrder[j] != currentButtonOrder[j])
                    {
                        currentShapeOrder = new ShapeType[5];
                        currentButtonOrder = new ButtonType[5];
                        entryNum = 0;
                        return;
                    }
                }

                currentShapeOrder = new ShapeType[5];
                currentButtonOrder = new ButtonType[5];
                entryNum = 0;
                hiddenSwitches[i].activateMethod.Invoke();
                return;
            }
        }
    }

    // private void Activate(ShapeType shape)
    // {
    //     switch(shape)
    //     {
    //         case ShapeType.Triangle:
    //             _isTriangleActive = !_isTriangleActive;
    //             StartMovingObject(firstBridges, -20f, 0f);
    //             AngelCreation(triangleChancesOfAngel, triangleMaxAngels);
    //             break;

    //         case ShapeType.Square:
    //             _isSquareActive = !_isSquareActive;
    //             AngelCreation(squareChancesOfAngel, squareMaxAngels);
    //             break;

    //         case ShapeType.Circle:
    //             _isCircleActive = !_isCircleActive;
    //             DisableMotion();
    //             AngelCreation(circleChancesOfAngel, circleMaxAngels);
    //             break;

    //         case ShapeType.Pentagon:
    //             _isPentagonActive = !_isPentagonActive;
    //             StartMovingObject(signs, -1f, 4f);
    //             AngelCreation(pentagonChancesOfAngel, pentagonMaxAngels);
    //             break;

    //         case ShapeType.Diamond:
    //             _isDiamondActive = !_isDiamondActive;
    //             DrainFlashlightCharge();
    //             AngelCreation(diamondChancesOfAngel, diamondMaxAngels);
    //             break;
    //     }

    //     if (_isSquareActive && _isDiamondActive && Mathf.Abs(secondBridges.position.y - 0) > 1)
    //     {
    //         StartMovingObject(secondBridges, -20f, 0f);
    //     }
    // }

    private void DisableMotion()
    {
        PCPlayerInputManager.ToggleRestriction("Move", false);
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

    private void StartMovingObject(Transform currentObject, float minPosition, float maxPosition)
    {
        _isMovingObject = !_isMovingObject;
        movingObject = currentObject;

        lerpTargetY = -100;
        if (movingObject.position.y == maxPosition)
        {
            lerpTargetY = minPosition;
        }else if (movingObject.position.y == minPosition)
        {
            lerpTargetY = maxPosition;
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
