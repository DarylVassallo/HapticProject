using UnityEngine;
using System.Collections;

public class DrawerButtonTrigger : MonoBehaviour
{
    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _originalMaterial;

    [SerializeField] private bool isSquare;
    [SerializeField] private bool isCircle;
    [SerializeField] private bool isTriangle;
    [SerializeField] private bool isCross;
    [SerializeField] private bool isStar;

    private bool _canBePressed;
    private Collider _enteredCollider;

    private void Awake()
    {
        _canBePressed = true;
    }

    private void OnTriggerEnter(Collider _other)
    {
        Debug.Log(this.gameObject + ": OnTriggerEnter: " + _other);

        if(_canBePressed && (_other.gameObject.layer == LayerMask.NameToLayer("LeftHandPhysics") || _other.gameObject.layer == LayerMask.NameToLayer("RightHandPhysics")))
        {
            Debug.Log(this.gameObject + ": Pressed");

            if(buttonRenderer.material != _highlightMaterial) StartCoroutine(HighlightButton(0.25f, _originalMaterial, _highlightMaterial));

            if(_other.gameObject.layer == LayerMask.NameToLayer("LeftHandPhysics"))
            {
                EventsManager.PingVRController(0);
            }else if(_other.gameObject.layer == LayerMask.NameToLayer("RightHandPhysics"))
            {
                EventsManager.PingVRController(1);
            }

            _enteredCollider = _other;
            _canBePressed = false;

            if(isSquare)
            {
                Debug.Log(this.gameObject + ": Square");
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Square);
            }else if(isCircle)
            {
                Debug.Log(this.gameObject + ": Circle");
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Circle);
            }else if(isTriangle)
            {
                Debug.Log(this.gameObject + ": Triangle");
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Triangle);
            }else if(isCross)
            {
                Debug.Log(this.gameObject + ": Cross");
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Cross);
            }else if(isStar)
            {
                Debug.Log(this.gameObject + ": Star");
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Star);
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if(_other == _enteredCollider)
        {
            _enteredCollider = null;
            _canBePressed = true;
            
            if(buttonRenderer.material != _originalMaterial) StartCoroutine(HighlightButton(0.25f, _highlightMaterial, _originalMaterial));
        }
    }

    IEnumerator HighlightButton(float _delay, Material _currentMaterial, Material _newMaterial)
    {
        float elapsed = 0f;

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            
            buttonRenderer.material.SetColor(
                "_BaseColor",
                Color.Lerp(
                    _currentMaterial.GetColor("_BaseColor"),
                    _newMaterial.GetColor("_BaseColor"),
                    elapsed / _delay
                )
            );    

            Debug.Log("1 buttonRenderer.material.color: " + buttonRenderer.material.color);          

            yield return null;
        }  

        buttonRenderer.material = _newMaterial;   
        Debug.Log("2 buttonRenderer.material: " + buttonRenderer.material);   
    }
}
