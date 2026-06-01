using UnityEngine;
using System;

using Unity.Netcode;

//This script has the health of the entity, and destroys it upon death.
public class Health : NetworkBehaviour
{
    private float _maxHealth = 100;
    private NetworkVariable<float> _health = new(100f);
    private NetworkVariable<int> _currentAttackerType = new(0);

    public static event Action OnGameOver;
    public static event Action<int, int> OnKilledEnemy;

    public static event Action<float> OnChangeHealthBar;
    public static event Action<float> OnChangeHealthCamera;

    private void Awake()
    {
        // SetHealthServerRpc(_maxHealth);
    }

    public override void OnNetworkSpawn()
    {
        _health.OnValueChanged += OnHealthChanged;
        SetHealthServerRpc(_maxHealth);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(float _newHealth)
    {
        _health.Value = _newHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAttackerTypeServerRpc(int _attackerType)
    {
        _currentAttackerType.Value = _attackerType;
    }

    public void ChangeHealth(float _healthChange, int _attackerType)
    {
        SetHealthServerRpc(_health.Value + _healthChange);
        if(_health.Value > _maxHealth) SetHealthServerRpc(_maxHealth);

        SetAttackerTypeServerRpc(_attackerType);
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if(newValue <= 0)
        {
            if (this.CompareTag("PCPlayer") || this.CompareTag("VRPlayer"))
            {
                OnGameOver?.Invoke();
            }
            else if (this.CompareTag("Enemy"))
            {
                OnKilledEnemy?.Invoke(1, _currentAttackerType.Value);
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (this.CompareTag("PCPlayer"))
            {
                OnChangeHealthBar?.Invoke(newValue);
                OnChangeHealthCamera?.Invoke(newValue);
            }
        }
    }

    public float GetHealth()
    {
        return _health.Value;
    }
}
