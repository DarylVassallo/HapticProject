using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 0;

    public void ChangeHealth(float _healthChange)
    {
        health += _healthChange;

        if(health <= 0) Destroy(this.gameObject);
    }
}
