using UnityEngine;
using System;

using Unity.Netcode;

//This script has the health of the entity, and destroys it upon death.
public class Health : NetworkBehaviour
{
    private float _maxHealth = 100;
    private NetworkVariable<float> _health = new(100f);

    public static event Action<Renderer, float> OnChangeEnemyOxidization;
    public static event Action<GameObject> OnEntityDeath;
    public static event Action<GameObject, float> OnEntityChangeHealth;

    private Animator _animator;

    private bool isDead;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        isDead = false;
    }

    public override void OnNetworkSpawn()
    {
        _health.OnValueChanged += OnHealthChanged;
        SetHealthServerRpc(_maxHealth);
    }

    private void OnEnable()
    {
        // HealthManager.OnChangeHealthForEntity += ChangeHealth;
        // HealthManager.OnResetHealthForEntity += ResetHealth;
    }

    private void OnDisable()
    {
        // HealthManager.OnResetHealthForEntity -= ResetHealth;
    }


    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(float _newHealth)
    {
        _health.Value = _newHealth;
        OnEntityChangeHealth?.Invoke(this.gameObject, _health.Value);
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
                isDead = true;
                OnEntityDeath?.Invoke(this.gameObject);

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
                    OnChangeEnemyOxidization?.Invoke(renderer, _health.Value / 100f);
                }
            }
        }
    }

    private float GetHealth()
    {
        return _health.Value;
    }
}
