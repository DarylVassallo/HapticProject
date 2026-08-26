using UnityEngine;
using System.Collections;

using Unity.Netcode;

public class TeleportButton : NetworkBehaviour, IInteractable
{
    private bool _isInteractable = true;    

    [SerializeField] private AudioClip buttonAudio;
    [SerializeField] private bool isForRope;
    private AudioSource _audioSource;

    private Vector3 targetPosition;
    private Vector3 originalPosition;
    private Vector3 pushedPosition;
    private float pressedDistance = 0.1f;
    private float _buttonSpeed = 20f;

    void Awake()
    {
        _audioSource = this.gameObject.GetComponent<AudioSource>();

        originalPosition = this.transform.position;
        pushedPosition =    this.transform.position + 
                            (   transform.forward * 
                                pressedDistance * 
                                this.transform.localScale.x
                            ); 
    }

    //Triggers interaction, if the PCPlayer selects the collectable
    public void TriggerInteraction()
    {
        if (_isInteractable)
        {
            if(isForRope)
            {
                EventsManager.TriggerRopeButton(this.transform);
            }
            else
            {
                EventsManager.TriggerTeleportButton(this.transform.parent.gameObject);
            }
            
            PushButtonRpc(0.5f, true);

            if(!_audioSource.isPlaying)
            {
                _audioSource.Stop();
                _audioSource.clip = buttonAudio;
                _audioSource.Play();
                _audioSource.enabled = true; 
            }
        }
    }

    public void EnableInteraction()
    {
        _isInteractable = true;
    }

    public void DisableInteraction()
    {
        _isInteractable = false;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PushButtonRpc(float _delay, bool _goDown)
    {
        StartCoroutine(PushButton(0.5f, true));
    }

    IEnumerator PushButton(float _delay, bool _goDown)
    {
        float elapsed = 0f;

        if(_goDown)
        {
            targetPosition = pushedPosition;
        } else {
            targetPosition = originalPosition;
        }

        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;

            this.transform.position = Vector3.Lerp(
                this.transform.position,
                targetPosition,
                Time.deltaTime * _buttonSpeed
            );

            yield return null;
        }

        if(_goDown)
        { 
            StartCoroutine(PushButton(0.5f, false));
        }
    }
}
