using UnityEngine;
//This script has the health of the entity, and destroys it upon death.
public class Health : MonoBehaviour
{
    public float health = 0;

    public void ChangeHealth(float _healthChange)
    {
        health += _healthChange;

        if(health <= 0) Destroy(this.gameObject);
    }
}
