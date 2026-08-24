using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private int addSymbolToCode;
    [SerializeField] private bool removeSymbol;
    [SerializeField] private bool inputSymbols;
    private bool _canBePressed;
    private Collider _enteredCollider;

    private void Awake()
    {
        _canBePressed = true;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if(_canBePressed && (_other.gameObject.layer == LayerMask.NameToLayer("LeftHandPhysics") || _other.gameObject.layer == LayerMask.NameToLayer("RightHandPhysics")))
        {
            _enteredCollider = _other;
            _canBePressed = false;

            if(addSymbolToCode != -1)
            {
                EventsManager.AddSymbolNumber(addSymbolToCode);
            }else if(removeSymbol)
            {
                EventsManager.RemoveSymbol();
            }else if(inputSymbols)
            {
                EventsManager.InputSymbols();
            }
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if(_other == _enteredCollider)
        {
            _enteredCollider = null;
            _canBePressed = true;
        }
    }
}
