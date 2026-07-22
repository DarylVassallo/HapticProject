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

    public static event Action OnResetButtons;

    public static event Action<int> OnCreateRandomEnemy;

    [SerializeField] private Transform[] rotateBridges;
    [SerializeField] private Transform[] reverseRotateBridges;

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
    
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material deactiveMaterial;

    [SerializeField] private GameObject[] secretBridges;

    private bool _isNewNavMeshAvailable;

    [SerializeField] private NavMeshSurface levelGround;

    private bool isFunctioningWheelRotating;

    [SerializeField] private AudioClip correctAudio;
    [SerializeField] private AudioClip incorrectAudio;
    [SerializeField] private AudioClip wheelAudio;
    [SerializeField] private AudioClip switchOnLeverAudio;
    [SerializeField] private AudioClip switchOffLeverAudio;
    private AudioSource _audioSource;

    private NetworkVariable<bool> _secretBridgeToggle = new (false);
    private bool prevSecretBridgeToggle = true;

    [SerializeField] private int maxWheelCheckCount;
    private int wheelCheckCount;

    [SerializeField] private bool startWithActivatedSquareWheel;
    [SerializeField] private bool startWithActivatedDiamondLever;
    [SerializeField] private bool startWithActivatedTriangleLever;
    [SerializeField] private bool startWithActivatedCircleButton;

    private NetworkVariable<int> _networkAudioNum = new (-1);

    public float testSpeed;

    void Awake()
    {        
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _isNewNavMeshAvailable = false;

        squareWheelKnob = squareWheelObject.GetComponentInChildren<XRKnob>();

        isFunctioningWheelRotating = false;

        wheelCheckCount = 0;
    }

    private void OnEnable()
    {
        ButtonInteract.OnTriggerButton += PressedButton;
        ButtonInteract.OnActivateReset += ResetButtons;

        _networkAudioNum.OnValueChanged += PlayAudio;

        CheckpointManager.OnResetHiddenSwitches += DeactivateAllServerRpc;

        // TeleportManager.OnEveythingCollected += ActivateCrookedBridges
    }

    private void OnDisable()
    {
        ButtonInteract.OnTriggerButton -= PressedButton;
        ButtonInteract.OnActivateReset -= ResetButtons;

        _networkAudioNum.OnValueChanged -= PlayAudio;

        CheckpointManager.OnResetHiddenSwitches -= DeactivateAllServerRpc;
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

        if(startWithActivatedSquareWheel) ActivateSquareWheel();
        if(startWithActivatedDiamondLever) ActivateDiamondLever();
        if(startWithActivatedTriangleLever) ActivateTriangleLever();
        if(startWithActivatedCircleButton) ActivateCircleButton();
    }

    void FixedUpdate()
    {
        if(!_audioSource.isPlaying && _networkAudioNum.Value != -1) SetAudioNumServerRpc(-1);
        
        if (prevSecretBridgeToggle != _secretBridgeToggle.Value)
        {
            prevSecretBridgeToggle = _secretBridgeToggle.Value;

            for(int i = 0; i < secretBridges.Length; i++)
            {
                foreach (Transform child in secretBridges[i].transform.GetComponentsInChildren<Transform>(true))
                {
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.enabled = _secretBridgeToggle.Value;

                    MeshCollider collider = child.GetComponent<MeshCollider>();
                    if (collider != null) collider.enabled = _secretBridgeToggle.Value;

                    RevealUnderLight reveal = child.GetComponent<RevealUnderLight>();
                    if (reveal != null) reveal.enabled = _secretBridgeToggle.Value;
                }
            }
        }
        

        if(isSquareWheelActive.Value)
        {
            CheckSquareWheel();

            for(int i = 0; i < rotateBridges.Length; i++)
            {
                rotateBridges[i].Rotate(0.0f, squareWheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
            }

            for(int i = 0; i < reverseRotateBridges.Length; i++)
            {
                reverseRotateBridges[i].Rotate(0.0f, -squareWheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
            }
        }

        // for(int i = 0; i < rotateBridges.Length; i++)
        // {
        //     rotateBridges[i].Rotate(0.0f, testSpeed * bridgeRotateSpeed, 0.0f, Space.Self);
        // }

        // for(int i = 0; i < reverseRotateBridges.Length; i++)
        // {
        //     reverseRotateBridges[i].Rotate(0.0f, -testSpeed * bridgeRotateSpeed, 0.0f, Space.Self);
        // }
    }

    private void PlayAudio(int previous, int current)
    {
        if(current == -1)
        {
            _audioSource.Stop();
            return;
        }
        
        AudioClip _currentAudio = incorrectAudio;
        switch (current)
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

        if(squareWheelKnobValueDiff != 0 && wheelCheckCount <= 0)
        {
            SetAudioNumServerRpc(2);
            // PlayAudio(2);

            _isNewNavMeshAvailable = true;
            // SetSquareWheelEnemyActiveServerRpc(true);
            // SetChanceOfEnemysServerRpc(0.01f);

            wheelCheckCount = maxWheelCheckCount;
        }
        else if(squareWheelKnobValueDiff == 0 && _isNewNavMeshAvailable && wheelCheckCount <= 0)
        {
            // StopAudioClientRpc();
            SetAudioNumServerRpc(-1);

            _isNewNavMeshAvailable = false;
            // levelGround.RemoveData();
            // levelGround.BuildNavMesh();

            // SetSquareWheelEnemyActiveServerRpc(false);
            // SetChanceOfEnemysServerRpc(-0.01f);
        }

        prevSquareWheelKnobValue = squareWheelKnob.value;
        if(wheelCheckCount > 0) wheelCheckCount--;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSecretBridgeEnableServerRpc()
    {
        _secretBridgeToggle.Value = !_secretBridgeToggle.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAudioNumServerRpc(int newAudioNum)
    {
        _networkAudioNum.Value = newAudioNum;
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
    private void DeactivateAllServerRpc()
    {
        isSquareWheelActive.Value = false;
        DeactivateObjectLight(squareWheelObject);

        isDiamondLeverActive.Value = false;
        DeactivateObjectLight(diamondLeverObject);

        isTriangleLeverActive.Value = false;
        DeactivateObjectLight(triangleLeverObject);

        isCircleButtonActive.Value = false;
        DeactivateObjectLight(circleButtonObject);

        ResetButtons();
    }

    // private void UpdateChancesOfEnemy(bool previous, bool current)
    // {
    //     float newChances =  0.01f * (squareWheelEnemyActive.Value ? 1 : 0) + 
    //                         0f * (diamondLeverEnemyActive.Value ? 1 : 0);

    //     SetChanceOfEnemysServerRpc(newChances);
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetChanceOfEnemysServerRpc(float _chance)
    // {
    //     chancesOfEnemy.Value = _chance;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetSquareWheelEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     squareWheelEnemyActive.Value = _isEnemyActive;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetDiamondLeverEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     diamondLeverEnemyActive.Value = _isEnemyActive;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetTriangleLeverEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     triangleLeverEnemyActive.Value = _isEnemyActive;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetCircleButtonEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     circleButtonEnemyActive.Value = _isEnemyActive;
    // }


    
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

    private void DeactivateObjectLight(Transform interactiveObject)
    {
        foreach (Renderer rend in interactiveObject.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.CompareTag("ActiveLight"))
            {
                rend.material = deactiveMaterial;
                break;
            }
        }
    }

    public void EnableDrainFlashlightCharge()
    {
        SetAudioNumServerRpc(3);
        // PlayAudio(3);

        if(!isDiamondLeverActive.Value) return;

        // SetDiamondLeverEnemyActiveServerRpc(true);
        if(_secretBridgeToggle.Value == false) SetSecretBridgeEnableServerRpc();

        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        EnableDrainFlashlight?.Invoke();
    }

    public void DisableDrainFlashlightCharge()
    {
        SetAudioNumServerRpc(4);
        // PlayAudio(4);

        if(!isDiamondLeverActive.Value) return;
        
        // SetDiamondLeverEnemyActiveServerRpc(false);
        if(_secretBridgeToggle.Value == true) SetSecretBridgeEnableServerRpc();
        
        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        DisableDrainFlashlight?.Invoke();
    }

    public void GivePCPlayerHealthByLever()
    {
        SetAudioNumServerRpc(3);
        // PlayAudio(3);

        if(isTriangleLeverActive.Value) GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Health>().ChangeHealth(6f, -1);
    }

    private void ResetButtons()
    {
        currentShapeOrder = new ShapeType[5];
        currentButtonOrder = new ButtonType[5];
        entryNum = 0;

        OnResetButtons?.Invoke();

        //Wrong
        return;
    }

    private void PressedButton(ShapeType _shape, ButtonType _button)
    {
        // InstantiateRandomEnemyServerRpc();
        OnCreateRandomEnemy?.Invoke(1);
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
                //Continue
                if(currentButtonOrder[currentButtonOrder.Length - 1] == ButtonType.None) return;

                for (int j = 0; j < hiddenSwitches[i].buttonOrder.Length; j++)
                {
                    if (hiddenSwitches[i].buttonOrder[j] != currentButtonOrder[j])
                    {
                        currentShapeOrder = new ShapeType[5];
                        currentButtonOrder = new ButtonType[5];
                        entryNum = 0;

                        OnResetButtons?.Invoke();

                        SetAudioNumServerRpc(0);
                        // PlayAudio(0);

                        //Wrong
                        return;
                    }
                }

                currentShapeOrder = new ShapeType[5];
                currentButtonOrder = new ButtonType[5];
                entryNum = 0;
                hiddenSwitches[i].activateMethod.Invoke();

                //Correct
                SetAudioNumServerRpc(1);
                // PlayAudio(1);

                return;
            }
        }
    }

    private void DisableMotion()
    {
        PCPlayerInputManager.ToggleRestriction("Move", false);
    }
}
