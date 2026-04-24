using UnityEngine;
using System;
//This script has the health of the entity, and destroys it upon death.
public class Health : MonoBehaviour
{
    public float health = 0;

    public static event Action OnGameOver;
    public static event Action<int> OnKilledEnemy;

    public void ChangeHealth(float _healthChange)
    {
        health += _healthChange;

        if(health <= 0)
        {
            if (this.CompareTag("Player"))
            {
                OnGameOver?.Invoke();
            }
            else if (this.CompareTag("Enemy"))
            {
                OnKilledEnemy?.Invoke(1);
                Destroy(this.gameObject);
            }
        } 
    }
}
