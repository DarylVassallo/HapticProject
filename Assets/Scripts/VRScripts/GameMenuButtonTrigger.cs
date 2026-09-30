using UnityEngine;

public class GameMenuButtonTrigger : MonoBehaviour
{
    [SerializeField] private bool isPauseButton;
    [SerializeField] private bool isBackButton;
    [SerializeField] private bool isEnglishButton;
    [SerializeField] private bool isFrenchButton;

    [SerializeField] private bool isResumeButton;
    [SerializeField] private bool isSettingsButton;
    [SerializeField] private bool isMainMenuButton;

    [SerializeField] private bool isRespawnButton;

    [SerializeField] private bool isRestartButton;

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

            if(isPauseButton)
            {
                EventsManager.Cancel(true);
            }else if(isBackButton)
            {
                EventsManager.Cancel(true);
                EventsManager.SettingsMenu(false);
            }else if(isEnglishButton)
            {
                EventsManager.ChangeLanguageToEnglish();
            }else if(isFrenchButton)
            {
                EventsManager.ChangeLanguageToFrench();



            }else if(isResumeButton)
            {
                EventsManager.Cancel(false);
            }else if(isSettingsButton)
            {
                EventsManager.Cancel(false);
                EventsManager.SettingsMenu(true);
            }else if(isMainMenuButton)
            {
                EventsManager.PlayMainMenuLevel();
            



            }else if(isRespawnButton)
            {
                EventsManager.Respawn();
            }




            else if(isRestartButton)
            {
                EventsManager.PlayGameLevel();
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
