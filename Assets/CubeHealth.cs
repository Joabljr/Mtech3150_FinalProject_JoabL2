using UnityEngine;

public class CubeHealth : MonoBehaviour
{
    [Header("Health")]
    public int health = 1;
    private int maxHealth;

    [Header("Rewards")]
    public bool givesSpecialAttack = false;
    public bool givesExtraJump = false;
    private bool rewardGiven = false;

    [Header("Sound Effects")]
    public AudioClip hitSound;
    public AudioClip breakSound;
    private AudioSource audioSource;

    [Header("Hit Flash Settings")]
    public Color hitColor = Color.white;
    public float flashDuration = 0.1f;
    public float flashLerpSpeed = 10f;

    private Renderer rend;
    private Color originalColor;
    private bool isFlashing = false;

    [Header("Health Color Settings")]
    public bool useHealthColors = true;
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Hit Animation")]
    public bool useScalePunch = true;
    public float punchAmount = 0.15f;
    public float punchSpeed = 10f;

    private Vector3 originalScale;

    private FirstPersonController playerController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.GetColor("_Color");

        originalScale = transform.localScale;

        maxHealth = health;

        playerController = FindFirstObjectByType<FirstPersonController>();

        UpdateHealthColor();
    }

    public void TakeHit(int damage)
    {
        TakeHit(damage, false);
    }

    public void TakeHit(int damage, bool fromEnemyLaser)
    {
        health -= damage;

        // 🔊 Hit sound
        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        // 🎨 Flash color
        if (!isFlashing)
            StartCoroutine(FlashColor());

        // 💥 Scale punch
        if (useScalePunch)
            StartCoroutine(PunchScale());

        // 🎨 Update health color
        UpdateHealthColor();

        // ⭐ Only handle death here
        if (health <= 0)
        {
            // 🔊 Break sound
            if (breakSound != null)
                AudioSource.PlayClipAtPoint(breakSound, transform.position, 1f);

            // ⭐ Voice line ONLY when cube actually breaks
            if (playerController != null)
                playerController.PlayVoice(playerController.vl_cubeDestroyed, playerController.freq_cubeDestroyed);

            // ⭐ Rewards (only if NOT enemy laser)
            if (!fromEnemyLaser && !rewardGiven)
            {
                rewardGiven = true;

                if (playerController != null)
                {
                    if (givesSpecialAttack)
                        playerController.GainSpecialAttack();

                    if (givesExtraJump)
                        playerController.GainExtraJump();
                }
            }

            Destroy(gameObject);
        }
    }

    // --- COLOR FLASH ---
    private System.Collections.IEnumerator FlashColor()
    {
        isFlashing = true;

        rend.material.SetColor("_Color", hitColor);

        yield return new WaitForSeconds(flashDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * flashLerpSpeed;
            rend.material.SetColor("_Color", Color.Lerp(hitColor, originalColor, t));
            yield return null;
        }

        rend.material.SetColor("_Color", originalColor);
        isFlashing = false;
    }

    // --- HEALTH COLOR ---
    private void UpdateHealthColor()
    {
        if (!useHealthColors || rend == null)
            return;

        float percent = (float)health / (float)maxHealth;

        Color target;

        if (percent > 0.66f)
            target = fullHealthColor;
        else if (percent > 0.33f)
            target = midHealthColor;
        else
            target = lowHealthColor;

        originalColor = target;
        rend.material.SetColor("_Color", target);
    }

    // --- SCALE PUNCH ---
    private System.Collections.IEnumerator PunchScale()
    {
        Vector3 targetScale = originalScale * (1f + punchAmount);
        float t = 0f;

        // Scale up
        while (t < 1f)
        {
            t += Time.deltaTime * punchSpeed;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        t = 0f;

        // Scale back
        while (t < 1f)
        {
            t += Time.deltaTime * punchSpeed;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
