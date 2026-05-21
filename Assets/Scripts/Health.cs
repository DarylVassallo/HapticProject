using UnityEngine;
using System;
//This script has the health of the entity, and destroys it upon death.
public class Health : MonoBehaviour
{
    private float _maxHealth = 100;
    private float _health;

    public static event Action OnGameOver;
    public static event Action<int, int> OnKilledEnemy;

    public static event Action<float> OnChangeHealthBar;
    public static event Action<float> OnChangeHealthCamera;

    private void Awake()
    {
        _health = _maxHealth;
    }

    public void ChangeHealth(float _healthChange, int _attackerType)
    {
        _health += _healthChange;

        if(_health > _maxHealth) _health = _maxHealth;

        if(_health <= 0)
        {
            if (this.CompareTag("PCPlayer") || this.CompareTag("VRPlayer"))
            {
                OnGameOver?.Invoke();
            }
            else if (this.CompareTag("Enemy"))
            {
                OnKilledEnemy?.Invoke(1, _attackerType);
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (this.CompareTag("PCPlayer"))
            {
                OnChangeHealthBar?.Invoke(_health);
                OnChangeHealthCamera?.Invoke(_health);
            }
        }
    }

    public float GetHealth()
    {
        return _health;
    }
}
