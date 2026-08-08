using UnityEngine;
using System;

using Unity.Netcode;

//This script has the health of the entity, and destroys it upon death.
public class Health : NetworkBehaviour
{
    private float _maxHealth = 100;
    private NetworkVariable<float> _health = new(100f);

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        _health.OnValueChanged += OnHealthChanged;
        SetHealthServerRpc(_maxHealth);
    }

    private void OnEnable()
    {
        EventsManager.OnChangeHealthForEntity += ChangeHealth;
        EventsManager.OnResetHealth += ResetHealth;
    }

    private void OnDisable()
    {
        EventsManager.OnChangeHealthForEntity -= ChangeHealth;
        EventsManager.OnResetHealth -= ResetHealth;
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetHealthServerRpc(float _newHealth)
    {
        _health.Value = _newHealth;
        EventsManager.EntityChangedHealth(this.gameObject, _health.Value);
    }

    //If this is the requested entity, then it's health is modified
    private void ChangeHealth(GameObject entity, float _healthChange)
    {
        if(entity == this.gameObject)
        {
            SetHealthServerRpc(_health.Value + _healthChange);
            if(_health.Value > _maxHealth) SetHealthServerRpc(_maxHealth);
        }
    }

    //If this is the requested entity, then it's health is reset
    private void ResetHealth(GameObject entity)
    {
        if(entity == this.gameObject) SetHealthServerRpc(_maxHealth);
    }
    private void OnHealthChanged(float previousValue, float newValue)
    {
        //If the health is gone, then the entity is removed, or if it is the PC Players, causes the players to lose 
        if(newValue <= 0)
        {
            if (this.CompareTag("PCPlayer") || this.CompareTag("VRPlayer"))
            {
                EventsManager.GameOver();
            }
            else if (this.CompareTag("Enemy"))
            {
                EventsManager.EntityKilled(this.gameObject);

                _animator.speed = 1;
                _animator.SetBool("IsDead", true);
            }

        //This applies a more intense health damage visual effect to the PC Player if they are the entity, 
        // or changes the visual appearance of the entity if it is an enemy
        } else {
            if (this.CompareTag("PCPlayer"))
            {
                EventsManager.ChangeHealthBar(newValue);
                EventsManager.ChangeHealthCamera(newValue);
            }else if (this.CompareTag("Enemy"))
            {
                foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>())
                {
                    EventsManager.ChangedEnemyOxidization(renderer, _health.Value / 100f);
                }
            }
        }
    }

    private float GetHealth()
    {
        return _health.Value;
    }
}
