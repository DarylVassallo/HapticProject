using UnityEngine;

using System;

//This script has the health of the entity, and destroys it upon death.
public class HealthManager : MonoBehaviour
{
    public static event Action<GameObject, float> OnChangeHealthForEntity;
    public static event Action<GameObject> OnResetHealthForEntity;

    public static event Action<GameObject> OnNotifyDeath;
    public static event Action<GameObject, float> OnNotifyChangedHealth;

    public static event Action OnGameOver;
    public static event Action<float> OnChangeHealthBar;

    public static event Action<Renderer, float> OnChangeEnemyOxidization;

    private void OnEnable()
    {
        EnemyWithSpotlight.OnChangeHealth += AssignChangeHealth;
        MazeManager.OnChangeHealth += AssignChangeHealth;
        FallTrigger.OnChangeHealth += AssignChangeHealth;

        Health.OnEntityDeath += NotifyDeath;
        Health.OnEntityChangeHealth += NotifyChangedHealth;
        Health.OnGameOverTrigger += GameOverTrigger;
        Health.OnChangeHealthBarTrigger += ChangeHealthBarTrigger;
        Health.OnChangeEnemyOxidizationTrigger += ChangeEnemyOxidizationTrigger;

        CheckpointManager.OnResetHealth += ResetHealth;
    }

    private void OnDisable()
    {
        EnemyWithSpotlight.OnChangeHealth -= AssignChangeHealth;
        MazeManager.OnChangeHealth -= AssignChangeHealth;
        FallTrigger.OnChangeHealth -= AssignChangeHealth;

        Health.OnEntityDeath -= NotifyDeath;
        Health.OnEntityChangeHealth -= NotifyChangedHealth;
        Health.OnGameOverTrigger -= GameOverTrigger;
        Health.OnChangeHealthBarTrigger -= ChangeHealthBarTrigger;
        Health.OnChangeEnemyOxidizationTrigger -= ChangeEnemyOxidizationTrigger;

        CheckpointManager.OnResetHealth -= ResetHealth;
    }

    //Assigns the health change for the requested entity
    private void AssignChangeHealth(GameObject entity, float damage)
    {
        OnChangeHealthForEntity?.Invoke(entity, damage);
    }

    //Resets the health of the requested entity
    private void ResetHealth(GameObject entity)
    {
        OnResetHealthForEntity?.Invoke(entity);
    }

    //Informs the requested entity of their death
    private void NotifyDeath(GameObject entity)
    {
        OnNotifyDeath?.Invoke(entity);
    }

    //Informs the requested entity of their changed health
    private void NotifyChangedHealth(GameObject entity, float newHealth)
    {
        OnNotifyChangedHealth?.Invoke(entity, newHealth);
    }

    private void GameOverTrigger()
    {
        OnGameOver?.Invoke();
    }
    
    private void ChangeHealthBarTrigger(float newValue)
    {
        OnChangeHealthBar?.Invoke(newValue);
    }

    private void ChangeEnemyOxidizationTrigger(Renderer rend, float newValue)
    {
        OnChangeEnemyOxidization?.Invoke(rend, newValue);
    }
}
