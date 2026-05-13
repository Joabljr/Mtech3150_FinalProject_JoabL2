using UnityEngine;

public class WarningIndicator : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            0.1f,
            player.position.z
        );
    }
}
