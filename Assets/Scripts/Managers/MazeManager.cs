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
    private AudioSource _audioSource;
    public static event Action<GameObject, float> OnChangeHealth;

    [Header("Switches")]
    public HiddenSwitches[] hiddenSwitches;

    //This is the Switch wall, recording the specific type of switch, 
    // and the button order required to activate it
    [System.Serializable]
    public struct HiddenSwitches
    {
        public ShapeType shape;
        public ButtonType[] buttonOrder;
        public UnityEvent activateMethod;
    }

    public enum ShapeType { None, Health, Spin, Defense }
    public enum ButtonType { None, Square, Circle, Triangle, Cross, Star }
    public enum InteractiveObject { SpinWheel, HealthBall, DefenseButton }

    private ShapeType[] currentShapeOrder = new ShapeType[5];
    private ButtonType[] currentButtonOrder = new ButtonType[5];
    private int entryNum = 0;

    public static event Action OnResetButtons;

    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material deactiveMaterial;
    [SerializeField] private AudioClip correctAudio;
    [SerializeField] private AudioClip incorrectAudio;
    [SerializeField] private bool startWithActivatedSpinWheel;
    [SerializeField] private bool startWithActivatedHealthBall;
    [SerializeField] private bool startWithActivatedDefenseButton;




    [Header("Spin Bridges")]
    [SerializeField] private Transform spinWheelObject;
    [SerializeField] private AudioClip wheelAudio;
    [SerializeField] private int maxWheelCheckCount;
    private NetworkVariable<int> _networkAudioNum = new (-1);
    private int wheelCheckCount;
    private NetworkVariable<bool> hasSpinWheelBeenUsed = new (false);
    private NetworkVariable<bool> isSpinWheelActive = new (false);
    private XRKnob spinWheelKnob;
    private float prevSpinWheelKnobValue = 0;
    private float spinWheelKnobValueDiff = 0;

    [SerializeField] private float bridgeRotateSpeed;
    [SerializeField] private Transform[] rotateBridges;
    [SerializeField] private Transform[] reverseRotateBridges;   

    private bool _isNewNavMeshAvailable;
    [SerializeField] private NavMeshSurface levelGround; 
    




    [Header("Health Ball")]
    [SerializeField] private Transform healthBallObject;
    public static event Action GivePCPlayerHealth;
    private NetworkVariable<bool> hasHealthBallBeenUsed = new (false);
    private NetworkVariable<bool> isHealthBallActive = new (false);

    public static event Action<int> OnCreateRandomEnemy;




    [Header("Defense Button")]
    [SerializeField] private Transform defenseButtonObject;
    private NetworkVariable<bool> hasDefenseButtonBeenUsed = new (false);
    private NetworkVariable<bool> isDefenseButtonActive = new (false);




    void Awake()
    {        
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        _isNewNavMeshAvailable = false;

        spinWheelKnob = spinWheelObject.GetComponentInChildren<XRKnob>();

        wheelCheckCount = 0;
    }

    private void OnEnable()
    {
        ButtonInteract.OnTriggerButton += PressedButton;
        ButtonInteract.OnActivateReset += ResetButtons;

        _networkAudioNum.OnValueChanged += PlayAudio;

        CheckpointManager.OnResetHiddenSwitches += DeactivateAllServerRpc;

        HealthBallTargeting.GivePCPlayerHealth += GivePCPlayerHealthUsingBall;
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
        isSpinWheelActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.SpinWheel, p, c);
        if(isSpinWheelActive.Value)
        {
            ActivateObjectLight(spinWheelObject);
        }

        isHealthBallActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.HealthBall, p, c);
        if(isHealthBallActive.Value)
        {
            ActivateObjectLight(healthBallObject);
        }

        isDefenseButtonActive.OnValueChanged += (p, c) => OnObjectChanged(InteractiveObject.DefenseButton, p, c);
        if(isDefenseButtonActive.Value)
        {
            ActivateObjectLight(defenseButtonObject);
        }

        base.OnNetworkSpawn();

        if(startWithActivatedSpinWheel) ActivateSpinWheel();
        if(startWithActivatedHealthBall) ActivateHealthBall();
        if(startWithActivatedDefenseButton) ActivateDefenseButton();
    }

    private void ActivateSpinWheel()
    {
        ActivateServerRpc(InteractiveObject.SpinWheel);
    }

    private void ActivateHealthBall()
    {
        ActivateServerRpc(InteractiveObject.HealthBall);
    }

    private void ActivateDefenseButton()
    {
        ActivateServerRpc(InteractiveObject.DefenseButton);
    }

    void FixedUpdate()
    {
        //Resets the audio number if audio is no longer being used
        if(!_audioSource.isPlaying && _networkAudioNum.Value != -1) SetAudioNumServerRpc(-1);
        
        //Rotates the bridges if the VR Player rotates the wheel, 
        // and the wheel is active
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
    }

    //Players / Stops the audio if requested (-1 means to stop audio, while other numbers refer to specific audio clips)
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

    //Creates a set number of enemies if the Defense Button is pressed (and if it is active)
    public void PressedDefenseButton(int maxEnemies)
    {
        if(isDefenseButtonActive.Value)
        {
            if(!hasDefenseButtonBeenUsed.Value) SetDefenseButtonBeenUsedServerRpc(true);

            OnCreateRandomEnemy?.Invoke(maxEnemies);
        }
    }

    //Checks if the wheel is rotating (plays rotating audio if it is rotating), 
    // or if it is idle
    private void CheckSpinWheel()
    {
        spinWheelKnobValueDiff = spinWheelKnob.value - prevSpinWheelKnobValue;

        if(spinWheelKnobValueDiff != 0 && wheelCheckCount <= 0)
        {
            SetAudioNumServerRpc(2);
            _isNewNavMeshAvailable = true;
            wheelCheckCount = maxWheelCheckCount;

            if(!hasSpinWheelBeenUsed.Value) SetSpinWheelBeenUsedServerRpc(true);
        }
        else if(spinWheelKnobValueDiff == 0 && _isNewNavMeshAvailable && wheelCheckCount <= 0)
        {
            SetAudioNumServerRpc(-1);
            _isNewNavMeshAvailable = false;
        }

        prevSpinWheelKnobValue = spinWheelKnob.value;
        if(wheelCheckCount > 0) wheelCheckCount--;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSpinWheelBeenUsedServerRpc(bool _newValue)
    {
        hasSpinWheelBeenUsed.Value = _newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthBallBeenUsedServerRpc(bool _newValue)
    {
        hasHealthBallBeenUsed.Value = _newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDefenseButtonBeenUsedServerRpc(bool _newValue)
    {
        hasDefenseButtonBeenUsed.Value = _newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAudioNumServerRpc(int newAudioNum)
    {
        _networkAudioNum.Value = newAudioNum;
    }
    
    //Activates a required interactive object (wheel, ball, or button), 
    // which allows them to effect the level
    [ServerRpc(RequireOwnership = false)]
    private void ActivateServerRpc(InteractiveObject interactiveObject)
    {
        switch(interactiveObject)
        {
            case InteractiveObject.SpinWheel:
                isSpinWheelActive.Value = true;
                break;

            case InteractiveObject.HealthBall:
                isHealthBallActive.Value = true;
                break;

            case InteractiveObject.DefenseButton:
                isDefenseButtonActive.Value = true;
                break;
        }
    }

    //Deactivates all interactive objects (wheel, ball, button), 
    // which prevents them from being able to effect the level
    [ServerRpc(RequireOwnership = false)]
    private void DeactivateAllServerRpc()
    {
        isSpinWheelActive.Value = false;
        DeactivateObjectLight(spinWheelObject);

        isHealthBallActive.Value = false;
        DeactivateObjectLight(healthBallObject);

        isDefenseButtonActive.Value = false;
        DeactivateObjectLight(defenseButtonObject);

        ResetButtons();
    }
    
    //Upon an interactive object being active, 
    // this causes its connected symbol to being switching color (between green and red) until the VR Player interacts with the object
    private void OnObjectChanged(InteractiveObject interactiveObject, bool previous, bool current)
    {
        if (!current) return;

        switch(interactiveObject)
        {
            case InteractiveObject.SpinWheel:
                ActivateObjectLight(spinWheelObject);
                break;

            case InteractiveObject.HealthBall:
                ActivateObjectLight(healthBallObject);
                break;

            case InteractiveObject.DefenseButton:
                ActivateObjectLight(defenseButtonObject);
                break;
        }
    }

    //Switches the interactive object's symbol to green, 
    // then waits until the symbol can be switched to red
    private void ActivateObjectLight(Transform interactiveObject)
    {
        foreach (Renderer rend in interactiveObject.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.CompareTag("ActiveLight"))
            {
                rend.material.SetColor("_BaseColor", Color.green);
                StartCoroutine(ObjectLightFlicker(false, rend.material, interactiveObject));
                break;
            }
        }
    }

    //Switches the interactive object's symbol to red, 
    // then waits until the symbol can be switched to green
    private void DeactivateObjectLight(Transform interactiveObject)
    {
        foreach (Renderer rend in interactiveObject.GetComponentsInChildren<Renderer>(true))
        {
            if (rend.CompareTag("ActiveLight"))
            {
                rend.material.SetColor("_BaseColor", Color.red);
                break;
            }
        }
    }

    //Upon an object's symbol switching colour, 
    // this triggers a delay where after 1 second checks if the object has been used yet, 
    // and if not continues switching the symbol's colour. 
    // If the object has been used then the colour is set to green.
    private IEnumerator ObjectLightFlicker(bool isOn, Material objectLight, Transform interactiveObject)
    {
        bool hasBeenUsed = false;

        yield return new WaitForSeconds(1f);

        if(interactiveObject == spinWheelObject)
        {
            hasBeenUsed = hasSpinWheelBeenUsed.Value;
        }else if(interactiveObject == healthBallObject)
        {
            hasBeenUsed = hasHealthBallBeenUsed.Value;
        }else if(interactiveObject == defenseButtonObject)
        {
            hasBeenUsed = hasDefenseButtonBeenUsed.Value;
        }

        if(!hasBeenUsed)
        {
            if(isOn)
            {
                objectLight.SetColor("_BaseColor", Color.red);
                StartCoroutine(ObjectLightFlicker(false, objectLight, interactiveObject));
            }
            else
            {
                objectLight.SetColor("_BaseColor", Color.green);
                StartCoroutine(ObjectLightFlicker(true, objectLight, interactiveObject));
            }
        }
        else
        {
            objectLight.SetColor("_BaseColor", Color.green);
        }
    }

    //If the health ball (interactive ball) collides with the PC Player (and is active), 
    // this function will give the PC Player some health
    private void GivePCPlayerHealthUsingBall()
    {
        SetAudioNumServerRpc(3);

        if(isHealthBallActive.Value)
        {
            if(!hasHealthBallBeenUsed.Value) SetHealthBallBeenUsedServerRpc(true);

            OnChangeHealth?.Invoke(GameObject.FindGameObjectWithTag("PCPlayer"), 6f);
        }
    }

    //Resets the recorded inputted buttons of the switch
    private void ResetButtons()
    {
        currentShapeOrder = new ShapeType[5];
        currentButtonOrder = new ButtonType[5];
        entryNum = 0;

        OnResetButtons?.Invoke();

        //Wrong
        return;
    }

    //Records the new inputted button of the switch, 
    // Checks if the button matches the switches order, 
    // Checks if the button belongs to the current switch,
    // Spawns one enemy nearby
    private void PressedButton(ShapeType _shape, ButtonType _button)
    {
        //Spawns one enemy nearby 
        OnCreateRandomEnemy?.Invoke(1);

        //Checks if buttons have already been inputted. 
        // If they have been, it checks if all buttons belong to the same switch. 
        // If they do not, then the entry is reset.
        for(int i = 0; i < currentShapeOrder.Length; i++)
        {
            if(currentShapeOrder[i] == ShapeType.None) break;

            if(currentShapeOrder[i] != _shape)
            {
                ResetButtons();
                break;
            }
        }

        //Adds the new input to the current order
        currentShapeOrder[entryNum] = _shape;
        currentButtonOrder[entryNum] = _button;
        entryNum++;

        //Checks if the current order is correct. 
        // If it is wrong, then the buttons are reset. 
        // If the order is correct and complete, then the connected interactive object is activated
        for (int i = 0; i < hiddenSwitches.Length; i++)
        {
            if(hiddenSwitches[i].shape == _shape)
            {
                //The checked switch is the one being used

                //Checks if the current order is complete
                if(currentButtonOrder[currentButtonOrder.Length - 1] == ButtonType.None) return;

                for (int j = 0; j < hiddenSwitches[i].buttonOrder.Length; j++)
                {
                    if (hiddenSwitches[i].buttonOrder[j] != currentButtonOrder[j])
                    {
                        //Current order does not match the switch's order

                        ResetButtons();
                        SetAudioNumServerRpc(0);
                        return;
                    }
                }

                //The current order is complete and correct
                ResetButtons();
                hiddenSwitches[i].activateMethod.Invoke();
                SetAudioNumServerRpc(1);

                return;
            }
        }
    }
}
