using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;

    public void TakeSpecialDamage(int amount)
    {
        health -= amount;
        Debug.Log("Enemy took SPECIAL damage, health = " + health);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}