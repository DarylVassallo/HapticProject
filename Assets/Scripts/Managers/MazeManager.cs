using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VRTemplate;

using UnityEngine.Events;
using Unity.AI.Navigation;

using Unity.Netcode;

//This controls the various things that could occur due to the hidden switches
public class MazeManager : NetworkBehaviour
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

    public enum ShapeType { None, Triangle, Square, Circle, Pentagon, Diamond }
    public enum ButtonType { None, Square, Circle, Triangle, Cross, Star }
    public enum InteractiveObject { SquareWheel, DiamondLever, TriangleLever, CircleButton }

    private ShapeType[] currentShapeOrder = new ShapeType[5];
    private ButtonType[] currentButtonOrder = new ButtonType[5];
    private int entryNum = 0;

    public static event Action EnableDrainFlashlight;
    public static event Action DisableDrainFlashlight;
    public static event Action GivePCPlayerHealth;

    [SerializeField] private Transform halfBridges;

    [SerializeField] private Transform squareWheelObject;
    private NetworkVariable<bool> isSquareWheelActive = new (false);
    private XRKnob squareWheelKnob;
    private float prevSquareWheelKnobValue = 0;
    private float squareWheelKnobValueDiff = 0;

    [SerializeField] private Transform diamondLeverObject;
    private NetworkVariable<bool> isDiamondLeverActive = new (false);

    [SerializeField] private Transform triangleLeverObject;
    private NetworkVariable<bool> isTriangleLeverActive = new (false);

    [SerializeField] private Transform circleButtonObject;
    private NetworkVariable<bool> isCircleButtonActive = new (false);

    private bool canPCFunction = false;

    
    [SerializeField] private Material activeMaterial;

    [SerializeField] private GameObject crookedBridge;

    private bool _isNewNavMeshAvailable;

    [SerializeField] private NavMeshSurface levelGround;

    private NetworkVariable<float> chancesOfAngel = new(0f);

    private bool isFunctioningWheelRotating;

    [SerializeField] private AudioClip correctAudio;
    [SerializeField] private AudioClip incorrectAudio;
    [SerializeField] private AudioClip wheelAudio;
    [SerializeField] private AudioClip switchOnLeverAudio;
    [SerializeField] private AudioClip switchOffLeverAudio;
    private AudioSource _audioSource;

    private NetworkVariable<bool> _crookedBridgeToggle = new (false);

    void Awake()
    {        
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _isNewNavMeshAvailable = false;

        _closeSpawnPoints = new List<Transform>();
        squareWheelKnob = squareWheelObject.GetComponentInChildren<XRKnob>();

        isFunctioningWheelRotating = false;
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

    public override void OnNetworkSpawn()
    {
        isSquareWheelActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.SquareWheel, p, c);
        if(isSquareWheelActive.Value)
        {
            ActivateObjectLight(squareWheelObject);
        }

        isDiamondLeverActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.DiamondLever, p, c);
        if(isDiamondLeverActive.Value)
        {
            ActivateObjectLight(diamondLeverObject);
        }

        isTriangleLeverActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.TriangleLever, p, c);
        if(isTriangleLeverActive.Value)
        {
            ActivateObjectLight(triangleLeverObject);
        }

        isCircleButtonActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.CircleButton, p, c);
        if(isCircleButtonActive.Value)
        {
            ActivateObjectLight(circleButtonObject);
        }

        base.OnNetworkSpawn();
    }

    void FixedUpdate()
    {
        if(chancesOfAngel.Value > 0) PotentialAngelCreation();

        if(!canPCFunction || (!isSquareWheelActive.Value && !isDiamondLeverActive.Value)) return;

        if(crookedBridge.activeSelf != _crookedBridgeToggle.Value)
        {
            crookedBridge.SetActive(_crookedBridgeToggle.Value);
        }

        if(isSquareWheelActive.Value)
        {
            CheckSquareWheel();
            halfBridges.Rotate(0.0f, squareWheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
        }
    }

    [ClientRpc]
    private void PlayAudioClientRpc(int _audioNum)
    {
        AudioClip _currentAudio = incorrectAudio;
        switch (_audioNum)
        {
            case 0:
                _currentAudio = incorrectAudio;
                break;
            case 1:
                _currentAudio = correctAudio;
                break;
            case 2:
                _currentAudio = wheelAudio;
                break;
            case 3:
                _currentAudio = switchOnLeverAudio;
                break;
            case 4:
                _currentAudio = switchOffLeverAudio;
                break;
        }

        if(!_audioSource.isPlaying)
        {
            _audioSource.Stop();
            _audioSource.clip = _currentAudio;
            _audioSource.Play();
            _audioSource.enabled = true; 
        }
    }

    [ClientRpc]
    private void StopAudioClientRpc()
    {
        _audioSource.Stop();
    }

    private void CheckSquareWheel()
    {
        squareWheelKnobValueDiff = squareWheelKnob.value - prevSquareWheelKnobValue;

        if(squareWheelKnobValueDiff != 0)
        {
            PlayAudioClientRpc(2);
            _isNewNavMeshAvailable = true;
            AddChanceOfAngelsServerRpc(0.1f);
        }
        else if(squareWheelKnobValueDiff == 0 && _isNewNavMeshAvailable)
        {
            StopAudioClientRpc();

            _isNewNavMeshAvailable = false;
            levelGround.RemoveData();
            levelGround.BuildNavMesh();

            AddChanceOfAngelsServerRpc(-0.1f);
        }

        prevSquareWheelKnobValue = squareWheelKnob.value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCrookedBridgeEnableServerRpc()
    {
        _crookedBridgeToggle.Value = !_crookedBridgeToggle.Value;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ActivateServerRpc(InteractiveObject interactiveObject)
    {
        switch(interactiveObject)
        {
            case InteractiveObject.SquareWheel:
                isSquareWheelActive.Value = true;
                break;

            case InteractiveObject.DiamondLever:
                isDiamondLeverActive.Value = true;
                break;

            case InteractiveObject.TriangleLever:
                isTriangleLeverActive.Value = true;
                break;

            case InteractiveObject.CircleButton:
                isCircleButtonActive.Value = true;
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddChanceOfAngelsServerRpc(float _chance)
    {
        chancesOfAngel.Value = chancesOfAngel.Value + _chance;
    }
    
    private void OnObjectChanged(InteractiveObject interactiveObject, bool previous, bool current)
    {
        if (!current) return;

        switch(interactiveObject)
        {
            case InteractiveObject.SquareWheel:
                ActivateObjectLight(squareWheelObject);
                break;

            case InteractiveObject.DiamondLever:
                ActivateObjectLight(diamondLeverObject);
                break;

            case InteractiveObject.TriangleLever:
                ActivateObjectLight(triangleLeverObject);
                break;

            case InteractiveObject.CircleButton:
                ActivateObjectLight(circleButtonObject);
                break;
        }
    }
    
    public void ActivateSquareWheel()
    {
        ActivateServerRpc(InteractiveObject.SquareWheel);
    }

    public void ActivateDiamondLever()
    {
        ActivateServerRpc(InteractiveObject.DiamondLever);
    }

    public void ActivateTriangleLever()
    {
        ActivateServerRpc(InteractiveObject.TriangleLever);
    }

    public void ActivateCircleButton()
    {
        ActivateServerRpc(InteractiveObject.CircleButton);
    }

    private void ActivateObjectLight(Transform interactiveObject)
    {
        foreach (Renderer rend in interactiveObject.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.CompareTag("ActiveLight"))
            {
                rend.material = activeMaterial;
                break;
            }
        }
    }

    public void EnableDrainFlashlightCharge()
    {
        PlayAudioClientRpc(3);

        if(!isDiamondLeverActive.Value) return;

        if(_crookedBridgeToggle == false) SetCrookedBridgeEnableServerRpc();

        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        EnableDrainFlashlight?.Invoke();
    }

    public void DisableDrainFlashlightCharge()
    {
        PlayAudioClientRpc(4);

        if(!isDiamondLeverActive.Value) return;
        
        if(_crookedBridgeToggle == true) SetCrookedBridgeEnableServerRpc();
        
        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        DisableDrainFlashlight?.Invoke();
    }

    public void GivePCPlayerHealthByLever()
    {
        PlayAudioClientRpc(3);

        GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Health>().ChangeHealth(2f, -1);
    }

    private void PressedButton(ShapeType _shape, ButtonType _button)
    {
        InstantiateRandomAngelServerRpc();
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
        currentButtonOrder[entryNum] = _button;
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

                        Debug.Log("INCORRECT");
                        PlayAudioClientRpc(0);

                        return;
                    }
                }

                currentShapeOrder = new ShapeType[5];
                currentButtonOrder = new ButtonType[5];
                entryNum = 0;
                hiddenSwitches[i].activateMethod.Invoke();

                Debug.Log("CORRECT");
                PlayAudioClientRpc(1);

                return;
            }
        }
    }

    private void DisableMotion()
    {
        PCPlayerInputManager.ToggleRestriction("Move", false);
    }

    public void AngelCreation(int maxAngels)
    {
        int _numberOfAngels = UnityEngine.Random.Range(1, maxAngels);

        for (int i = 0; i < _numberOfAngels; i++)
        {
            InstantiateRandomAngelServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InstantiateRandomAngelServerRpc()
    {
        int angelNum = UnityEngine.Random.Range(1, angelSpawnPoints.childCount) - 1;
        var newAngel = Instantiate(angel, angelSpawnPoints.GetChild(angelNum).position, Quaternion.identity);
        newAngel.GetComponent<NetworkObject>().Spawn();
    }

    private void PotentialAngelCreation()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= chancesOfAngel.Value) InstantiateRandomAngelServerRpc();
    }

    private void InstantiateNearbyRandomAngel()
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
