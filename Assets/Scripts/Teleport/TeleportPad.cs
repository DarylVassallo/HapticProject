using UnityEngine;
using TMPro;

using Unity.Netcode;
using System;
using System.Collections;

public class TeleportPad : NetworkBehaviour
{
    [SerializeField] private Transform exitTeleportPad;

    private Vector3 _positionDifference;

    private NetworkVariable<bool> _isPlayerOnPad = new (false);
    private Transform _pcPlayerTransform;

    private TeleportManager _teleportManager;

    [SerializeField] private Transform bar;

    private bool arePadsReady = true;

    private bool _hasBeenUsed = false;

    [SerializeField] private bool _instantTeleport;
    [SerializeField] private bool _effectsEnemies;

    private bool rotateRings = false;

    [SerializeField] private Transform bubble;
    private Material _bubbleMaterial;

    [SerializeField] private Transform ring;
    [SerializeField] private Transform reverseRing;
    private float rotateSpeed = 1f;
    [SerializeField] private float rotateIncrement = 1f;
    
    [SerializeField] private float minRotateSpeed;
    [SerializeField] private float maxRotateSpeed;

    private Quaternion restRotation;
    [SerializeField] private float rotationRange;

    [SerializeField] private bool _isFinalPad;

    private AudioSource _audioSource;
    [SerializeField] private AudioClip _teleportAudio;

    private bool _isTutorialTeleport;

    [Header("General")]
    [SerializeField] private Transform parentDisplay;
    private String _stringSecretCode;
    
    [Header("Symbols")]
    [SerializeField] private bool usesSymbols;
    private Renderer[] displayedSymbols;
    [SerializeField] private Material clearSymbol;
    [SerializeField] private Material[] potentialSymbols;

    [Header("Levers")]
    [SerializeField] private bool usesLevers;
    private Transform[] displayedNotches;
    [SerializeField] private Transform inputDisplay;
    private Transform[] inputNotches;
    private float neutralYPos;
    [SerializeField] private float topYPos;
    [SerializeField] private float bottomYPos;
    private bool[] inputLevers;
    private bool[] answerLevers;
    
    [Header("Rope")]
    [SerializeField] private bool usesRope;

    private int[] answerButtons;
    private int currentButtonCount;

    [SerializeField] private Transform hidingBar;
    private Transform[] pieces;
    [SerializeField] private Transform leftHidingTransform;
    [SerializeField] private Transform middleHidingTransform;
    [SerializeField] private Transform rightHidingTransform;

    [SerializeField] private Transform buttonParentsObject;
    [SerializeField] private Material neutralButtonMaterial;
    [SerializeField] private Material activeButtonMaterial;
    private Transform[] buttons;

    [SerializeField] private Transform lightsParentsObject;
    [SerializeField] private Material deactiveLightMaterial;
    [SerializeField] private Material activeLightMaterial;
    private Transform[] lights;

    void Start()
    {
        _bubbleMaterial = bubble.GetComponent<Renderer>().material;

        bubble.localScale = new Vector3(8, 8, 8);
        Color colour = _bubbleMaterial.color;
        colour.a = 0;
        _bubbleMaterial.color = colour;

        if(bar != null) EventsManager.AddNewBar(bar);
    }

    private void OnEnable()
    {
        EventsManager.OnSendCodeToTeleportPads += CheckInputtedCode;
        EventsManager.OnEverythingCollected += ActivateInstantTeleport;
        EventsManager.OnResetTeleportPads += ResetTeleportPad;

        EventsManager.OnChangePadsReady += ChangePadsReady;

        EventsManager.OnTriggerTeleportButton += CheckPad;

        EventsManager.OnSetExitPadTransform += SetExitPadTransform;

        if(usesLevers)
        {
            EventsManager.OnActivateLever += ActivateInputNotchServerRpc;
            EventsManager.OnDeactivateLever += DeactivateInputNotchServerRpc;
        }

        if(usesRope)
        {
            EventsManager.OnChangeHidingBarPosition += ChangeHidingBarPositionRpc;
            EventsManager.OnTriggerRopeButton += TriggerRopeButton;
        }
    }

    private void OnDisable()
    {
        EventsManager.OnSendCodeToTeleportPads -= CheckInputtedCode;
        EventsManager.OnEverythingCollected -= ActivateInstantTeleport;
        EventsManager.OnResetTeleportPads -= ResetTeleportPad;

        EventsManager.OnChangePadsReady -= ChangePadsReady;

        EventsManager.OnTriggerTeleportButton -= CheckPad;

        EventsManager.OnSetExitPadTransform -= SetExitPadTransform;

        if(usesLevers)
        {
            EventsManager.OnActivateLever -= ActivateInputNotchServerRpc;
            EventsManager.OnDeactivateLever -= DeactivateInputNotchServerRpc;
        }

        if(usesRope)
        {
            EventsManager.OnChangeHidingBarPosition -= ChangeHidingBarPositionRpc;
            EventsManager.OnTriggerRopeButton -= TriggerRopeButton;
        }
    }

    public override void OnNetworkSpawn()
    {
        _stringSecretCode = "";

        if(parentDisplay != null)
        {
            if(usesSymbols)
            {
                displayedSymbols = new Renderer[parentDisplay.childCount];
                for (int i = 0; i < parentDisplay.childCount; i++)
                {
                    displayedSymbols[i] = parentDisplay.GetChild(i).GetComponent<Renderer>();
                    displayedSymbols[i].material = clearSymbol;
                } 
            }else if(usesLevers)
            {
                displayedNotches = new Transform[parentDisplay.childCount];
                for (int i = 0; i < parentDisplay.childCount; i++)
                {
                    displayedNotches[i] = parentDisplay.GetChild(i);

                    if(i == 0)
                    {
                        neutralYPos = displayedNotches[i].position.y;
                        topYPos = neutralYPos + topYPos;
                        bottomYPos = neutralYPos + bottomYPos;
                    }
                } 

                inputNotches = new Transform[inputDisplay.childCount];
                for (int i = 0; i < inputDisplay.childCount; i++)
                {
                    inputNotches[i] = inputDisplay.GetChild(i);
                    MoveNotchRpc(true, i, true);
                } 

                inputLevers = new bool[5];
                answerLevers = new bool[5];
                for(int i = 0; i < inputLevers.Length; i++)
                {
                    inputLevers[i] = false;
                    answerLevers[i] = false;
                }
            }else if(usesRope)
            {
                buttons = new Transform[buttonParentsObject.childCount];
                answerButtons = new int[buttonParentsObject.childCount];

                lights = new Transform[lightsParentsObject.childCount];

                currentButtonCount = 0;

                for (int i = 0; i < buttonParentsObject.childCount; i++)
                {
                    buttons[i] = buttonParentsObject.GetChild(i);
                    buttons[i].GetComponent<Renderer>().material = neutralButtonMaterial;
                    answerButtons[i] = -1;

                    lights[i] = lightsParentsObject.GetChild(i);
                    lights[i].GetComponent<Renderer>().material = deactiveLightMaterial;
                } 

                pieces = new Transform[hidingBar.childCount];
                for (int i = 0; i < hidingBar.childCount; i++)
                {
                    pieces[i] = hidingBar.GetChild(i);
                }

                CheckMesh();
            }
        }

        _teleportManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<TeleportManager>();
        restRotation = ring.rotation;
        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }


    //Resets the teleport pad, so the PC Player can use it 'for the first time' again
    private void ResetTeleportPad()
    {
        _hasBeenUsed = false;
    }

    //This triggers the teleportation sequence immediately without requiring the code
    private void ActivateInstantTeleport()
    {
        bar.parent.gameObject.SetActive(false);
        _instantTeleport = true;
    }

    //This allows the TeleportManager to change the current teleport pad's exit pad
    private void SetExitPadTransform(GameObject _pad, Transform _newExitPadTransform)
    {
        if(_pad == this.gameObject)
        {
            exitTeleportPad = _newExitPadTransform;
            ChangeSecretCodePadServerRpc();
        }
    }

    //This changes the code to a random number (within the set range)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void  ChangeSecretCodePadServerRpc()
    {
        if(usesSymbols)
        {
            _stringSecretCode = "";
            for(int i = 0; i < 4; i++)
            {
                int _newNum = UnityEngine.Random.Range(0, 4);
                _stringSecretCode = _stringSecretCode + "" + (_newNum + 1);

                // displayedSymbols[i].material = potentialSymbols[_newNum];
                ChangeSymbolMaterialRpc(i, _newNum);
            }
        }else if(usesLevers)
        {
            for(int i = 0; i < 5; i++)
            {
                int _newNum = UnityEngine.Random.Range(0, 2);

                switch(_newNum)
                {
                    case 0:
                        MoveNotchRpc(false, i, true);
                        answerLevers[i] = false;
                        break;
                    case 1:
                        MoveNotchRpc(false, i, false);
                        answerLevers[i] = true;
                        break;
                }
            }
        }else if(usesRope)
        {            
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i] = -1;
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i] = UnityEngine.Random.Range(0, 5);

                for (int j = 0; j < answerButtons.Length; j++)
                {
                    if(i != j && answerButtons[i] == answerButtons[j])
                    {
                        i--;
                        j = answerButtons.Length;
                    }
                }
            } 

            ClearRopeButtons();
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void MoveNotchRpc(bool isInputNotch, int _index, bool _toTheBottom)
    {
        float yPos;
        if(_toTheBottom)
        {
            yPos = bottomYPos;
        }
        else
        {
            yPos = topYPos;
        }

        if(isInputNotch)
        {
            StartCoroutine(MoveNotch(1f, inputNotches[_index], new Vector3(inputNotches[_index].position.x, yPos, inputNotches[_index].position.z)));
        }else{
            StartCoroutine(MoveNotch(1f, displayedNotches[_index], new Vector3(displayedNotches[_index].position.x, yPos, displayedNotches[_index].position.z)));
        }
    }

    private void ClearRopeButtons()
    {
        currentButtonCount = 0;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            ChangeRopeButtonMaterialRpc(0, i, 0);
            ChangeRopeButtonMaterialRpc(1, i, 0);
        }

        ChangeRopeButtonMaterialRpc(0, answerButtons[0], 1);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeSymbolMaterialRpc(int _displayIndex, int _potentialIndex)
    {
        displayedSymbols[_displayIndex].material = potentialSymbols[_potentialIndex];
    }
    
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeRopeButtonMaterialRpc(int _objectType, int _buttonIndex, int _materialIndex)
    {
        switch(_objectType)
        {
            case 0:
                switch(_materialIndex)
                {
                    case 0:
                        buttons[_buttonIndex].GetComponent<Renderer>().material = neutralButtonMaterial;
                        break;
                    case 1:
                        buttons[_buttonIndex].GetComponent<Renderer>().material = activeButtonMaterial;
                        break;
                }
                break;
            case 1:
                switch(_materialIndex)
                {
                    case 0:
                        lights[_buttonIndex].GetComponent<Renderer>().material = deactiveLightMaterial;
                        break;
                    case 1:
                        lights[_buttonIndex].GetComponent<Renderer>().material = activeLightMaterial;
                        break;
                }
                break;
        }
        
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void  ActivateInputNotchServerRpc(int _leverIndex)
    {
        inputLevers[_leverIndex] = true;
        MoveNotchRpc(true, _leverIndex, false);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void  DeactivateInputNotchServerRpc(int _leverIndex)
    {
        inputLevers[_leverIndex] = false;
        MoveNotchRpc(true, _leverIndex, true);
    }

    IEnumerator MoveNotch(float _delay, Transform notch, Vector3 newPos)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            notch.position = Vector3.Lerp(
                notch.position,
                newPos,
                elapsed / _delay
            );

            yield return null;
        }

        notch.position = newPos;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void  ChangeHidingBarPositionRpc(float _hidingbarPosition)
    {
        hidingBar.position = Vector3.Lerp(
                leftHidingTransform.position,
                rightHidingTransform.position,
                _hidingbarPosition
            );

        CheckMesh();
    }

    private void CheckMesh()
    {
        float distance;
        for(int i = 0; i < pieces.Length; i++)
        {
            distance = Vector3.Distance(pieces[i].position, middleHidingTransform.position);
            if(distance >= 1f || distance <= -1f)
            {
                pieces[i].GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                pieces[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void  ChangeIsPlayerOnPadServerRpc(bool _newOnPad)
    {
        _isPlayerOnPad.Value = _newOnPad;
    }

    //If the inputted code is correct, then the teleportation sequence can begin
    private void CheckInputtedCode(int _inputtedCode)
    {
        if(usesSymbols)
        {
            if(_stringSecretCode == "") return;

            int _intSecretCode = int.Parse(_stringSecretCode);
            
            if(_inputtedCode == _intSecretCode && _isPlayerOnPad.Value)
            {
                StartRingRotationRpc();
            }
        }else if(usesLevers)
        {
            bool _leversMatch = true;

            for(int i = 0; i < 5; i++)
            {
                if(inputLevers[i] != answerLevers[i]) _leversMatch = false;
            }
            
            if(_leversMatch && _isPlayerOnPad.Value)
            {
                StartRingRotationRpc();
            }
        }
    }

    private void TriggerRopeButton(Transform button)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            if(buttons[i] == button)
            {
                CheckPressedRopeButtonServerRpc(i);
                return;
            }
        }

        ClearRopeButtons();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CheckPressedRopeButtonServerRpc(int _index)
    {
        if(answerButtons[currentButtonCount] == _index)
        {
            ChangeRopeButtonMaterialRpc(1, currentButtonCount, 1);
            ChangeRopeButtonMaterialRpc(0, _index, 0);

            if(currentButtonCount >= (buttons.Length - 1) && _isPlayerOnPad.Value)
            {
                ClearRopeButtons();
                StartRingRotationRpc();
            }
            else
            {
                currentButtonCount++;
                ChangeRopeButtonMaterialRpc(0, answerButtons[currentButtonCount], 1);
            }

            return;
        }
    }

    //If the inputted code is correct, then the teleportation sequence can begin
    private void CheckPad(GameObject _teleportPad)
    {
        if(_teleportPad == this.gameObject)
        {
            _isTutorialTeleport = true;
            StartRingRotationRpc();
        }
    }

    private void OnTriggerEnter(Collider _other)
    {
        //If the PC Player has entered this teleport pad for the first time, the chances for an enemy to randomly spawn increases slightly
        if (!_hasBeenUsed)
        {
            _hasBeenUsed = true;
            if(_effectsEnemies)
            {
                EventsManager.IncreaseChanceOfSpawningEnemy(0.0001f);
            }

            //If the PCPlayer has reached the final teleport pad, their progress is saved
            if(_isFinalPad)
            {
                EventsManager.CrossedCrookedBridges();
            }
        }

        //This saves if the PC Player has entered this teleport pad, and can also begin the teleport sequence immediately if required
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(true);

            if(_instantTeleport)
            {
                StartRingRotationRpc();
            }
        }
    }

    //This causes the rings of both the current and exit teleport pad to begin rotation, while playing the teleportation audio
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void StartRingRotationRpc()
    {
        if(!arePadsReady) return;

        if(exitTeleportPad != null) exitTeleportPad.GetComponent<TeleportPad>().rotateRings = true;
        rotateRings = true;

        _audioSource.Stop();
        _audioSource.clip = _teleportAudio;
        _audioSource.pitch = 3f;
        _audioSource.Play();
        _audioSource.enabled = true; 
    }

    //Stops the audio playing
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void StopAudioRpc()
    {
        _audioSource.Stop();
    }

    //Once ready, this teleports the PC Player to the exit pad, in the same position and rotation relative to the current teleport pad
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TeleportRpc()
    {
        if(!arePadsReady) return;
        
        EventsManager.ChangePadsReady(false);

        StartCoroutine(TeleportPause());

        if(_pcPlayerTransform == null) _pcPlayerTransform = GameObject.FindGameObjectWithTag("PCPlayer").transform;

        Vector3 localPos = this.transform.InverseTransformPoint(_pcPlayerTransform.position);
        Quaternion localRot = Quaternion.Inverse(this.transform.rotation) * _pcPlayerTransform.GetChild(0).rotation;

        _pcPlayerTransform.position = exitTeleportPad.TransformPoint(localPos);
        _pcPlayerTransform.GetChild(0).rotation = exitTeleportPad.rotation * localRot;
    }

    private void FixedUpdate()
    {
        if(!rotateRings) return;
        
        RotateRings();
        ChangeTeleportEffect();
        CheckTeleportCondition();
    }

    private void RotateRings()
    {
        //This rotates the rings
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || ring.rotation != restRotation) ring.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        //This rotates the rings in the opposite direction
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed || reverseRing.rotation != restRotation) reverseRing.Rotate(Vector3.up * -rotateSpeed * Time.deltaTime);
    }

    private void ChangeTeleportEffect()
    {
        //This increases/decreases the rotation speed and teleportation visual effect (visuals only applied to the PC Player)
        if(rotateIncrement > 0 || rotateSpeed > minRotateSpeed)
        {
            float _newScale = Mathf.Lerp(
                15f,
                4f,
                (rotateSpeed - minRotateSpeed) / (maxRotateSpeed - minRotateSpeed)
            );
            bubble.localScale = new Vector3(_newScale, _newScale, _newScale);

            float _newAlpha = Mathf.Lerp(
                0f,
                1f,
                (rotateSpeed - minRotateSpeed) / (maxRotateSpeed - minRotateSpeed)
            );
            Color colour = _bubbleMaterial.color;
            colour.g = 1 - _newAlpha;
            colour.a = _newAlpha;
            _bubbleMaterial.color = colour;

            rotateSpeed += rotateIncrement;
            EventsManager.ChangeTeleportRotateSpeed(rotateSpeed - minRotateSpeed, maxRotateSpeed - minRotateSpeed);
        }
        else
        {
            float _newScale = 80;
            bubble.localScale = new Vector3(_newScale, _newScale, _newScale);

            Color colour = _bubbleMaterial.color;
            colour.a = 0f;
            _bubbleMaterial.color = colour;
        }
    }

    private void CheckTeleportCondition()
    {
        //Inverts the rotation increment to begin reducing rotation speed, and teleports the PC Player
        if(rotateSpeed >= maxRotateSpeed)
        {
            rotateIncrement *= -1f;   
            if(_isTutorialTeleport)
            {
                EventsManager.TutorialTeleport();
            }else if(_isPlayerOnPad.Value)
            {
                 TeleportRpc();   
            }  
        }

        //If the rotation has slowed down enough, and the rings are at a rough angle, the rotation is stopped
        if( rotateIncrement < 0 && 
            rotateSpeed <= minRotateSpeed && 
            Quaternion.Angle(ring.rotation, restRotation) < rotationRange && 
            Quaternion.Angle(reverseRing.rotation, restRotation) < rotationRange)
        {
            rotateIncrement *= -1f;  
            rotateRings = false;

            ring.rotation = restRotation;
            reverseRing.rotation = restRotation;

            EventsManager.ChangeTeleportRotateSpeed(0, maxRotateSpeed);
            
            StopAudioRpc();
        }
    }

    //Records if the PC Player has left the teleport pad
    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("PCPlayer"))
        {
            ChangeIsPlayerOnPadServerRpc(false);
        }
    }

    //This stops the teleport pad from working for a period of time, to avoid multiple teleports at once
    IEnumerator TeleportPause()
    {
        yield return new WaitForSeconds(5f);

        if(rotateRings)
        {
            StartCoroutine(TeleportPause());
        }
        else
        {
            EventsManager.ChangePadsReady(true);
        }
    }

    private void ChangePadsReady(bool _newPadsReady)
    {
        arePadsReady = _newPadsReady;
    }
}
