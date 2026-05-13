using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    private Vector3 targetPoint;
    private Vector3 directionToPlayer;
    public float rotationSpeed = 5f;

    public float viewAngle = 10;

    public float viewRange = 5;

    public LayerMask playerLayer;

    public float detectionRadius = 2f;
    
    private CharacterController characterController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);
        
        targetPoint = new Vector3(player.position.x, transform.position.y, player.position.z);
        directionToPlayer = (targetPoint - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(directionToPlayer);

if (PlayerDetected())
        {
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rot, rotationSpeed * Time.deltaTime);
        }
    }

    /*controller.Move(transform .forward * walkSpeed * Time.deltaTime);*/

    private bool PlayerDetected()
    {
        bool result = false;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle < viewAngle / 2)
        {
            if (Physics.Raycast(transform.position, directionToPlayer, viewRange, playerLayer))
            {
                
                    result = true;
            }
        }
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRadius)
        {
            result = true;
        }
        
        return result;
    }
}

/* Hw- Pacing, back and forth - grayboxing(probuilder) */