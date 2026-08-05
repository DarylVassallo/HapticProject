using UnityEngine;
using System.Collections;
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

    public enum ShapeType { None, Health, Spin, Defense }
    public enum ButtonType { None, Square, Circle, Triangle, Cross, Star }
    public enum InteractiveObject { SpinWheel, HealthLever, DefenseButton }

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

    [SerializeField] private Transform spinWheelObject;
    private NetworkVariable<bool> hasSpinWheelBeenUsed = new (false);
    private NetworkVariable<bool> isSpinWheelActive = new (false);
    private XRKnob spinWheelKnob;
    private float prevSpinWheelKnobValue = 0;
    private float spinWheelKnobValueDiff = 0;

    [SerializeField] private Transform healthLeverObject;
    private NetworkVariable<bool> hasHealthLeverBeenUsed = new (false);
    private NetworkVariable<bool> isHealthLeverActive = new (false);

    [SerializeField] private Transform defenseButtonObject;
    private NetworkVariable<bool> hasDefenseButtonBeenUsed = new (false);
    private NetworkVariable<bool> isDefenseButtonActive = new (false);
    
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

    [SerializeField] private bool startWithActivatedSpinWheel;
    [SerializeField] private bool startWithActivatedHealthLever;
    [SerializeField] private bool startWithActivatedDefenseButton;

    private NetworkVariable<int> _networkAudioNum = new (-1);

    public float testSpeed;

    void Awake()
    {        
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _isNewNavMeshAvailable = false;

        spinWheelKnob = spinWheelObject.GetComponentInChildren<XRKnob>();

        isFunctioningWheelRotating = false;

        wheelCheckCount = 0;
    }

    private void OnEnable()
    {
        ButtonInteract.OnTriggerButton += PressedButton;
        ButtonInteract.OnActivateReset += ResetButtons;

        _networkAudioNum.OnValueChanged += PlayAudio;

        CheckpointManager.OnResetHiddenSwitches += DeactivateAllServerRpc;

        HealthBallTargeting.GivePCPlayerHealth += GivePCPlayerHealthByLever;

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
        Debug.Log("OnNetworkSpawn");
        isSpinWheelActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.SpinWheel, p, c);
        if(isSpinWheelActive.Value)
        {
            ActivateObjectLight(spinWheelObject);
        }

        isHealthLeverActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.HealthLever, p, c);
        if(isHealthLeverActive.Value)
        {
            ActivateObjectLight(healthLeverObject);
        }

        isDefenseButtonActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.DefenseButton, p, c);
        if(isDefenseButtonActive.Value)
        {
            ActivateObjectLight(defenseButtonObject);
        }

        base.OnNetworkSpawn();

        Debug.Log("OnNetworkSpawn 2");
        if(startWithActivatedSpinWheel) ActivateSpinWheel();
        if(startWithActivatedHealthLever) ActivateHealthLever();
        if(startWithActivatedDefenseButton) ActivateDefenseButton();
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
        

        if(isSpinWheelActive.Value)
        {
            CheckSpinWheel();

            for(int i = 0; i < rotateBridges.Length; i++)
            {
                rotateBridges[i].Rotate(0.0f, spinWheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
            }

            for(int i = 0; i < reverseRotateBridges.Length; i++)
            {
                reverseRotateBridges[i].Rotate(0.0f, -spinWheelKnobValueDiff * bridgeRotateSpeed, 0.0f, Space.Self);
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

    public void PressedDefenseButton(int maxEnemies)
    {
        if(isDefenseButtonActive.Value)
        {
            if(!hasDefenseButtonBeenUsed.Value) SetDefenseButtonBeenUsedServerRpc(true);

            OnCreateRandomEnemy?.Invoke(maxEnemies);
        }
    }
    private void CheckSpinWheel()
    {
        spinWheelKnobValueDiff = spinWheelKnob.value - prevSpinWheelKnobValue;

        if(spinWheelKnobValueDiff != 0 && wheelCheckCount <= 0)
        {
            SetAudioNumServerRpc(2);
            // PlayAudio(2);

            _isNewNavMeshAvailable = true;
            // SetSpinWheelEnemyActiveServerRpc(true);
            // SetChanceOfEnemysServerRpc(0.01f);

            wheelCheckCount = maxWheelCheckCount;

            if(!hasSpinWheelBeenUsed.Value)
            {
                Debug.Log("CheckSpinWheel " + spinWheelKnobValueDiff + " : " + wheelCheckCount);
                SetSpinWheelBeenUsedServerRpc(true);
            }
        }
        else if(spinWheelKnobValueDiff == 0 && _isNewNavMeshAvailable && wheelCheckCount <= 0)
        {
            // StopAudioClientRpc();
            SetAudioNumServerRpc(-1);

            _isNewNavMeshAvailable = false;
            // levelGround.RemoveData();
            // levelGround.BuildNavMesh();

            // SetSpinWheelEnemyActiveServerRpc(false);
            // SetChanceOfEnemysServerRpc(-0.01f);
        }

        prevSpinWheelKnobValue = spinWheelKnob.value;
        if(wheelCheckCount > 0) wheelCheckCount--;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSpinWheelBeenUsedServerRpc(bool _newValue)
    {
        Debug.Log("SetSpinWheelBeenUsedServerRpc: " + _newValue);
        hasSpinWheelBeenUsed.Value = _newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthLeverBeenUsedServerRpc(bool _newValue)
    {
        hasHealthLeverBeenUsed.Value = _newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDefenseButtonBeenUsedServerRpc(bool _newValue)
    {
        hasDefenseButtonBeenUsed.Value = _newValue;
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
        Debug.Log("ActivateServerRpc: " + interactiveObject);

        switch(interactiveObject)
        {
            case InteractiveObject.SpinWheel:
                isSpinWheelActive.Value = true;
                break;

            case InteractiveObject.HealthLever:
                isHealthLeverActive.Value = true;
                break;

            case InteractiveObject.DefenseButton:
                isDefenseButtonActive.Value = true;
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateAllServerRpc()
    {
        isSpinWheelActive.Value = false;
        DeactivateObjectLight(spinWheelObject);

        isHealthLeverActive.Value = false;
        DeactivateObjectLight(healthLeverObject);

        isDefenseButtonActive.Value = false;
        DeactivateObjectLight(defenseButtonObject);

        ResetButtons();
    }

    // private void UpdateChancesOfEnemy(bool previous, bool current)
    // {
    //     float newChances =  0.01f * (spinWheelEnemyActive.Value ? 1 : 0);

    //     SetChanceOfEnemysServerRpc(newChances);
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetChanceOfEnemysServerRpc(float _chance)
    // {
    //     chancesOfEnemy.Value = _chance;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetSpinWheelEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     spinWheelEnemyActive.Value = _isEnemyActive;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetHealthLeverEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     healthLeverEnemyActive.Value = _isEnemyActive;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void SetDefenseButtonEnemyActiveServerRpc(bool _isEnemyActive)
    // {
    //     defenseButtonEnemyActive.Value = _isEnemyActive;
    // }


    
    private void OnObjectChanged(InteractiveObject interactiveObject, bool previous, bool current)
    {
        Debug.Log("OnObjectChanged: " + interactiveObject + " : " + current);
        if (!current) return;

        switch(interactiveObject)
        {
            case InteractiveObject.SpinWheel:
                ActivateObjectLight(spinWheelObject);
                break;

            case InteractiveObject.HealthLever:
                ActivateObjectLight(healthLeverObject);
                break;

            case InteractiveObject.DefenseButton:
                ActivateObjectLight(defenseButtonObject);
                break;
        }
    }
    
    public void ActivateSpinWheel()
    {
        Debug.Log("ActivateSpinWheel");
        ActivateServerRpc(InteractiveObject.SpinWheel);
    }

    public void ActivateHealthLever()
    {
        ActivateServerRpc(InteractiveObject.HealthLever);
    }

    public void ActivateDefenseButton()
    {
        ActivateServerRpc(InteractiveObject.DefenseButton);
    }

    private void ActivateObjectLight(Transform interactiveObject)
    {
        Debug.Log("ActivateObjectLight: " + interactiveObject);
        foreach (Renderer rend in interactiveObject.GetComponentsInChildren<Renderer>(true))
        {
            Debug.Log("ActivateObjectLight foreach");
            if (rend.CompareTag("ActiveLight"))
            {
                Debug.Log("ActivateObjectLight tag");
                // rend.material = activeMaterial;
                rend.material.SetColor("_BaseColor", Color.green);
                Debug.Log("ObjectLightFlicker 1");
                StartCoroutine(ObjectLightFlicker(false, rend.material, interactiveObject));
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
                // rend.material = deactiveMaterial;
                rend.material.SetColor("_BaseColor", Color.red);
                break;
            }
        }
    }

    private IEnumerator ObjectLightFlicker(bool isOn, Material objectLight, Transform interactiveObject)
    {
        Debug.Log("Running ObjectLightFlicker: " + isOn + " : " + objectLight + " : " + interactiveObject);
        bool hasBeenUsed = false;

        yield return new WaitForSeconds(1f);

        if(interactiveObject == spinWheelObject)
        {
            hasBeenUsed = hasSpinWheelBeenUsed.Value;
        }else if(interactiveObject == healthLeverObject)
        {
            hasBeenUsed = hasHealthLeverBeenUsed.Value;
        }else if(interactiveObject == defenseButtonObject)
        {
            hasBeenUsed = hasDefenseButtonBeenUsed.Value;
        }

        if(!hasBeenUsed)
        {
            if(isOn)
            {
                objectLight.SetColor("_BaseColor", Color.red);
                Debug.Log("ObjectLightFlicker 2");
                StartCoroutine(ObjectLightFlicker(false, objectLight, interactiveObject));
            }
            else
            {
                objectLight.SetColor("_BaseColor", Color.green);
                Debug.Log("ObjectLightFlicker 3");
                StartCoroutine(ObjectLightFlicker(true, objectLight, interactiveObject));
            }
        }
        else
        {
            objectLight.SetColor("_BaseColor", Color.green);
        }
    }

    public void EnableDrainFlashlightCharge()
    {
        SetAudioNumServerRpc(3);
        // PlayAudio(3);

        if(_secretBridgeToggle.Value == false) SetSecretBridgeEnableServerRpc();

        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        EnableDrainFlashlight?.Invoke();
    }

    public void DisableDrainFlashlightCharge()
    {
        SetAudioNumServerRpc(4);
        // PlayAudio(4);
        
        if(_secretBridgeToggle.Value == true) SetSecretBridgeEnableServerRpc();
        
        levelGround.RemoveData();
        levelGround.BuildNavMesh();

        DisableDrainFlashlight?.Invoke();
    }

    public void GivePCPlayerHealthByLever()
    {
        SetAudioNumServerRpc(3);
        // PlayAudio(3);

        if(isHealthLeverActive.Value)
        {
            if(!hasHealthLeverBeenUsed.Value) SetHealthLeverBeenUsedServerRpc(true);
            GameObject.FindGameObjectWithTag("PCPlayer").GetComponent<Health>().ChangeHealth(6f, -1);
        }
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
