using UnityEngine;

public class SuperJumpPickup : MonoBehaviour
{
    private bool used = false;

    void OnDestroy()
    {
        if (used) return;
        used = true;

        // ✔ Updated to the new Unity API
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();

        if (player != null)
        {
            player.extraJumps += 1;   // ⭐ Give 1 extra jump
            Debug.Log("Player gained an EXTRA JUMP!");
        }
    }
}
