using UnityEngine;
using System;
//This script has the health of the entity, and destroys it upon death.
public class Health : MonoBehaviour
{
    private float health = 100;

    public static event Action OnGameOver;
    public static event Action<int, int> OnKilledEnemy;

    public static event Action<float> OnChangeHealthBar;

    public void ChangeHealth(float _healthChange, int _attackerType)
    {
        health += _healthChange;

        if(health <= 0)
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
            if (this.CompareTag("PCPlayer")) OnChangeHealthBar?.Invoke(health);
        }
    }

    public float GetHealth()
    {
        return health;
    }
}
