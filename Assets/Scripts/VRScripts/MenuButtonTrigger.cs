using UnityEngine;

public class MenuButtonTrigger : MonoBehaviour
{
    [SerializeField] private bool back;
    
    [Header("Network Mode")]
    [SerializeField] private bool playAlone;
    [SerializeField] private bool playTogether;

    [Header("Main Menu")]
    [SerializeField] private bool startGame;
    [SerializeField] private bool settings;
    [SerializeField] private bool credits;
    [SerializeField] private bool quit;

    [Header("Settings Menu")]
    [SerializeField] private bool readInEnglish;
    [SerializeField] private bool readInFrench;

    private int _currentMenuNum;

    private bool _canBePressed;
    private Collider _enteredCollider;

    private void Awake()
    {
        _canBePressed = true;
        _currentMenuNum = -1;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if(_canBePressed && (_other.gameObject.layer == LayerMask.NameToLayer("LeftHandPhysics") || _other.gameObject.layer == LayerMask.NameToLayer("RightHandPhysics")))
        {
            _enteredCollider = _other;
            _canBePressed = false;

            if(back)
            {
                EventsManager.SetMenu(1);
                _currentMenuNum = 1;





            }else if(playAlone || playTogether)
            {
                EventsManager.HostButton();
                EventsManager.SetMenuWithoutNetwork(1);
                _currentMenuNum = 1;
                if(playAlone) EventsManager.RevealTemple();





            }else if(startGame)
            {
                EventsManager.PlayLevel("SauronLevelScene");
            }else if(settings)
            {
                EventsManager.SetMenu(2);
                _currentMenuNum = 2;
            }else if(credits)
            {
                EventsManager.SetMenu(3);
                _currentMenuNum = 3;
            }else if(quit)
            {
                EventsManager.Quit();





            }else if(readInEnglish)
            {
                EventsManager.ChangeLanguage(0);
            }else if(readInFrench)
            {
                EventsManager.ChangeLanguage(1);
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