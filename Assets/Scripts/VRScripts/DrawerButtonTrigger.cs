using UnityEngine;

public class DrawerButtonTrigger : MonoBehaviour
{
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
        if(_canBePressed && (_other.gameObject.layer == LayerMask.NameToLayer("LeftHandPhysics") || _other.gameObject.layer == LayerMask.NameToLayer("RightHandPhysics")))
        {
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
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Square);
            }else if(isCircle)
            {
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Circle);
            }else if(isTriangle)
            {
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Triangle);
            }else if(isCross)
            {
                EventsManager.PressedButtonHaptic(EventsManager.ButtonType.Cross);
            }else if(isStar)
            {
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
        }
    }
}
