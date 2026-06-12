using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

//This script updates the hidden object's shader to render correctly based on the spotlights position, direction, and angle (modified to include range) (modifications to the shader used chatgpt).
//Source: https://www.youtube.com/watch?v=ZjNmndbbT44
public class RevealUnderLight : MonoBehaviour
{
    [SerializeField] private bool isPCInteractable;
    [SerializeField] private bool isVRInteractable;
    [SerializeField] private bool isEffectedByLight;
    [SerializeField] private bool isReversed;

    public static event Action<GameObject, bool, bool, bool, bool> OnAddNewHiddenObject;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created.
    void Start()
    {
        Debug.Log(this.gameObject + " : Awake : " + this.gameObject + " , " + isPCInteractable + " , " + isVRInteractable + " , " + isEffectedByLight + " , " + isReversed);
        StartCoroutine(RequestNewHiddenObject());
    }

    IEnumerator RequestNewHiddenObject()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log(this.gameObject + " : RequestNewHiddenObject : " + this.gameObject + " , " + isPCInteractable + " , " + isVRInteractable + " , " + isEffectedByLight + " , " + isReversed);
        OnAddNewHiddenObject?.Invoke(this.gameObject, isPCInteractable, isVRInteractable, isEffectedByLight, isReversed);
    }
}