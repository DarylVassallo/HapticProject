using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

using Unity.Netcode;

//This script updates the hidden object's shader to render correctly based on the spotlights position, direction, and angle (modified to include range) (modifications to the shader used chatgpt).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class RevealUnderLightManager : NetworkBehaviour
{
    private Material _hiddenMaterial;
    
    private Transform _pcFlashLight;
    private FlashlightCharge _pcFlashLightCharge;
    private Transform _vrFlashLight;
    private FlashlightCharge _vrFlashLightCharge;

    private Vector3 _positionDifference;
    private float _positionDistance;
    private Vector3 _positionDirection;
    private Vector3 _flashlightDirection;
    private float _scale;
    private float _angleRad;
    private float _threshold;
    private float _range;
    private float _strength;
    private float _totalStrength;

    private bool _canPCFunction = false;
    private bool _canVRFunction = false;

    private float _strengthLimit;

    [Serializable]
    public class HiddenObject
    {
        public GameObject hiddenObject;
        public Collider hiddenCollider;
        public Renderer hiddenRenderer;
        public IInteractable hiddenIInteractable;
        public bool isPCInteractable;
        public bool isVRInteractable;
        public bool isEffectedByLight;
        public bool isReversed;
        public bool _isInteractable;
    }

    [SerializeField] private List<HiddenObject> hiddenObjects;
    
    public static event Action<bool> OnToggleAll;

    public NetworkVariable<int> storedOldHiddenObject = new(-1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created.
    void Awake()
    {
        // _hiddenMaterial = GetComponent<Renderer>().material;        

        // if (isReversed)
        // {
        //     _strengthLimit = 1f;
        // }else
        // {
        //     _strengthLimit = 0.15f;
        // }
    }

    private void OnEnable()
    {
        ConnectUIScript.OnCreatedPCPlayer += GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer += GetVRPlayerData;

        RevealUnderLight.OnAddNewHiddenObject += AddNewHiddenObject;

        HiddenTeleportButtonInteract.OnRemoveHiddenObject += RemoveHiddenObject;
        EnemyManager.OnRemoveHiddenObject += RemoveHiddenObject;

        Health.OnChangeEnemyOxidization += ChangeEnemyOxidization;
    }

    private void OnDisable()
    {
        ConnectUIScript.OnCreatedPCPlayer -= GetPCPlayerData;
        ConnectUIScript.OnCreatedVRPlayer -= GetVRPlayerData;

        RevealUnderLight.OnAddNewHiddenObject -= AddNewHiddenObject;

        HiddenTeleportButtonInteract.OnRemoveHiddenObject -= RemoveHiddenObject;
        EnemyManager.OnRemoveHiddenObject -= RemoveHiddenObject;

        Health.OnChangeEnemyOxidization -= ChangeEnemyOxidization;
    }

    private void GetPCPlayerData()
    {
        if(GameObject.FindGameObjectWithTag("PCPlayer") != null)
        {
            _canPCFunction = true;

            _pcFlashLight = GameObject.FindGameObjectWithTag("PCFlashLight").transform;
            _pcFlashLightCharge = GameObject.FindGameObjectWithTag("PCFlashLight").GetComponent<FlashlightCharge>();
        }
    }
    
    private void GetVRPlayerData()
    {
        if(GameObject.FindGameObjectWithTag("VRPlayer") != null)
        {
            _canVRFunction = true;

            _vrFlashLight = GameObject.FindGameObjectWithTag("VRFlashLight").transform;
            _vrFlashLightCharge = GameObject.FindGameObjectWithTag("VRFlashLight").GetComponent<FlashlightCharge>();
        }
    }

    private void AddNewHiddenObject(GameObject newHiddenObject, bool newIsPCInteractable, bool newIsVRInteractable, bool newIsEffectedByLight, bool newIsReversed)
    {
        // if(!IsOwner) return;

        Debug.Log("AddNewHiddenObject: " + newHiddenObject);
        hiddenObjects.Add(new HiddenObject
                        {
                            hiddenObject = newHiddenObject,
                            hiddenCollider = newHiddenObject.GetComponent<Collider>(),
                            hiddenRenderer = newHiddenObject.GetComponent<Renderer>(),
                            hiddenIInteractable = newHiddenObject.GetComponent<IInteractable>(),
                            isPCInteractable = newIsPCInteractable,
                            isVRInteractable = newIsVRInteractable,
                            isEffectedByLight = newIsEffectedByLight,
                            isReversed = newIsReversed,
                            _isInteractable = false
                        });
        Debug.Log("Add New Count: " + hiddenObjects.Count);
    }

    private void RemoveHiddenObject(GameObject oldHiddenObject)
    {
        Debug.Log("RemoveHiddenObject");
        for (int i = 0; i < hiddenObjects.Count; i++)
        {
            if (hiddenObjects[i].hiddenObject == oldHiddenObject)
            {
                Debug.Log("StoredOldHiddenObjectServerRpc: " + i);
                // storedOldHiddenObject = oldHiddenObject;
                StoredOldHiddenObjectServerRpc(i);
                
                Debug.Log("RemoveHiddenObjectDelay");
                RemoveHiddenObjectClientRpc();
            }
        }
    }

    [ClientRpc]
    public void RemoveHiddenObjectClientRpc()
    {
        // if(!IsOwner) return;

        Debug.Log("RemoveHiddenObject: " + storedOldHiddenObject.Value);
        hiddenObjects.Remove(hiddenObjects[storedOldHiddenObject.Value]);
        // hiddenObjects.RemoveAll(h => 
        //                         h.hiddenObject != null && 
        //                         h.hiddenObject.transform.IsChildOf(storedOldHiddenObject.transform));
        Debug.Log("Remove New Count: " + hiddenObjects.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StoredOldHiddenObjectServerRpc(int oldIndex)
    {
        Debug.Log("require StoredOldHiddenObjectServerRpc: " + oldIndex);
        storedOldHiddenObject.Value = oldIndex;
    }

    private void ChangeEnemyOxidization(Renderer _renderer, float oxidization)
    {
        _renderer.material.SetFloat("_MapBlend", 1 - oxidization);
    }

    private void Update()
    {
        // if(!IsOwner) return;

        if(!_canPCFunction) GetPCPlayerData();
        if(!_canVRFunction) GetVRPlayerData();

        // if(!_canVRFunction) return;

        // Debug.Log("update storedOldHiddenObject.Value: " + storedOldHiddenObject.Value);

        for(int i = 0; i < hiddenObjects.Count; i++)
        {
            if (hiddenObjects[i].isEffectedByLight)
            {
                if (CheckLightStrength(hiddenObjects[i]) >= (hiddenObjects[i].isReversed ? _strengthLimit = 1f : _strengthLimit = 0.15f))
                {
                    if(!hiddenObjects[i]._isInteractable)
                    {
                        hiddenObjects[i]._isInteractable = true;
                        if (hiddenObjects[i].hiddenCollider != null) hiddenObjects[i].hiddenCollider.enabled = true;
                        if (hiddenObjects[i].hiddenIInteractable != null) hiddenObjects[i].hiddenIInteractable.EnableInteraction();
                    }
                }
                else
                {
                    if(hiddenObjects[i]._isInteractable)
                    {
                        hiddenObjects[i]._isInteractable = false;
                        if (hiddenObjects[i].hiddenCollider != null) hiddenObjects[i].hiddenCollider.enabled = false;
                        if (hiddenObjects[i].hiddenIInteractable != null) hiddenObjects[i].hiddenIInteractable.DisableInteraction();
                    }
                }
            }
            
            Debug.Log("i: " + i);
            Debug.Log("hiddenObjects[i]: " + hiddenObjects[i]);
            Debug.Log("hiddenObjects[i].hiddenRenderer: " + hiddenObjects[i].hiddenRenderer);
            Debug.Log("hiddenObjects[i].hiddenRenderer.material: " + hiddenObjects[i].hiddenRenderer.material);
            Debug.Log("==========");

            if(hiddenObjects[i].hiddenRenderer.material)
            {
                if (_pcFlashLight == null)
                {
                    hiddenObjects[i].hiddenRenderer.material.SetFloat("_MyLightRange", 0);
                }else{
                    if(hiddenObjects[i].isPCInteractable)
                    {
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_PCLightPosition", _pcFlashLight.transform.position);
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_PCLightDirection", -_pcFlashLight.transform.forward);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_PCLightAngle", 55);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_PCLightRange", _pcFlashLightCharge.currentFlashLightRange * 0.5f);
                    }
                    else
                    {
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_PCLightPosition", new Vector3(0, 0, 0));
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_PCLightDirection", new Vector3(0, 0, 0));
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_PCLightAngle", 0f);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_PCLightRange", 0f);
                    }
                }

                if (_vrFlashLight == null)
                {
                    hiddenObjects[i].hiddenRenderer.material.SetFloat("_MyLightRange", 0);
                }else{
                    if(hiddenObjects[i].isVRInteractable)
                    {
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_VRLightPosition", _vrFlashLight.transform.position);
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_VRLightDirection", -_vrFlashLight.transform.forward);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_VRLightAngle", 55);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_VRLightRange", _vrFlashLightCharge.currentFlashLightRange * 0.5f);
                    }
                    else
                    {
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_VRLightPosition", new Vector3(0, 0, 0));
                        hiddenObjects[i].hiddenRenderer.material.SetVector("_VRLightDirection", new Vector3(0, 0, 0));
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_VRLightAngle", 0f);
                        hiddenObjects[i].hiddenRenderer.material.SetFloat("_VRLightRange", 0f);
                    }
                }
            }
        }
    }

    private float CheckPlayerLightStrength(Transform hiddenObject, Transform _flashLight)
    {
        _positionDifference = hiddenObject.position - _flashLight.position;
        _positionDistance = _positionDifference.magnitude;
        _positionDirection = _positionDifference.normalized;

        _flashlightDirection = _flashLight.transform.forward.normalized;

        _scale = Vector3.Dot(_positionDirection, _flashlightDirection);

        _angleRad = 55 * 0.5f * Mathf.Deg2Rad;
        _threshold = Mathf.Cos(_angleRad);

        _range = Mathf.Clamp01(1.0f - (_positionDistance * _positionDistance) / ((55 * 0.5f) * (55 * 0.5f)));
        _strength = Mathf.Clamp01((_scale - _threshold)) * _range;

        return _strength;
    }

    private float CheckLightStrength(HiddenObject _hiddenObject)
    {
        _totalStrength = 0;

        if(_hiddenObject.isVRInteractable && _vrFlashLight != null) _totalStrength += CheckPlayerLightStrength(_hiddenObject.hiddenRenderer.transform, _vrFlashLight);
        if(_hiddenObject.isPCInteractable && _pcFlashLight != null) _totalStrength += CheckPlayerLightStrength(_hiddenObject.hiddenRenderer.transform, _pcFlashLight);

        _totalStrength = Mathf.Clamp01(_totalStrength);

        if(_hiddenObject.isReversed) _totalStrength = 1 - _totalStrength;

        return _totalStrength;
    }
}