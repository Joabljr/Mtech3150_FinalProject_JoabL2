using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 40f;
    public float lifetime = 5f;
    public int damage = 1;

    // Direction assigned by the player controller
    public Vector3 shootDirection;

    void Start()
    {
        Destroy(gameObject, lifetime);

        // Ignore ALL colliders on the player (camera, arms, body, etc.)
        Collider myCol = GetComponent<Collider>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            Collider[] cols = playerObj.GetComponentsInChildren<Collider>();
            foreach (Collider c in cols)
                Physics.IgnoreCollision(myCol, c);
        }
    }

    void Update()
    {
        transform.position += shootDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeSpecialDamage(damage);
            Destroy(gameObject);
        }
    }
}
