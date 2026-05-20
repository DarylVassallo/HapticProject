using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VRTemplate;

using UnityEngine.Events;
using Unity.AI.Navigation;

//This controls the various things that could occur due to the hidden switches
public class MazeManager : MonoBehaviour
{
    [SerializeField] private Transform signs;

    private bool _isMovingObject;
    private Transform movingObject;


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

    public static event Action EnableDrainFlashlight;
    public static event Action DisableDrainFlashlight;
    public static event Action GivePCPlayerHealth;

    [SerializeField] private Transform halfBridges;

    [SerializeField] private Transform wheelObject;
    private XRKnob wheelKnob;
    private bool isWheelActive;
    private float prevWheelKnobValue = 0;
    private float wheelKnobValueDiff = 0;

    [SerializeField] private Transform leverObject;
    private bool isLeverActive;

    private bool canPCFunction = false;

    
    [SerializeField] private Material activeMaterial;

    [SerializeField] private GameObject crookedBridge;

    private bool _isNewNavMeshAvailable = false;

    [SerializeField] private NavMeshSurface levelGround;
    void Awake()
    {        
        _closeSpawnPoints = new List<Transform>();
        wheelKnob = wheelObject.GetComponentInChildren<XRKnob>();
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
        if(!canPCFunction || (!isWheelActive)) return;

        if(isWheelActive)
        {
            wheelKnobValueDiff = wheelKnob.value - prevWheelKnobValue;
            halfBridges.Rotate(0.0f, wheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
            prevWheelKnobValue = wheelKnob.value;

            if(wheelKnobValueDiff > 0)
            {
                _isNewNavMeshAvailable = true;
            }else if(wheelKnobValueDiff == 0 && _isNewNavMeshAvailable)
            {
                _isNewNavMeshAvailable = false;
                levelGround.RemoveData();
                levelGround.BuildNavMesh();
            }
        }
    }

    public void ActivateWheel()
    {
        isWheelActive = true;
        wheelObject.Find("ActiveLight").GetComponent<Renderer>().material = activeMaterial;
    }

    public void ActivateLever()
    {
        isLeverActive = true;
        leverObject.Find("ActiveLight").GetComponent<Renderer>().material = activeMaterial;
    }

    public void EnableDrainFlashlightCharge()
    {
        crookedBridge.SetActive(true);
        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        EnableDrainFlashlight?.Invoke();
    }
    public void DisableDrainFlashlightCharge()
    {
        crookedBridge.SetActive(false);
        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        DisableDrainFlashlight?.Invoke();
    }

    public void GivePCPlayerHealthByLever()
    {
        GivePCPlayerHealth?.Invoke();
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
