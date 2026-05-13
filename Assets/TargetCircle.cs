using UnityEngine;

public class TargetCircle : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            0.1f, // force it to ground level
            player.position.z
        );
    }
}
