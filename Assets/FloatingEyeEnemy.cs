using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingEyeEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform platformCenter;
    public LineRenderer aimingLine;
    public LineRenderer chargeBeam;
    public LineRenderer laserBeam;

    [Header("Effects")]
    public ParticleSystem chargeParticles;
    public AudioSource chargeSound;
    public AudioSource fireSound;
    public CanvasGroup screenFlash;

    [Header("Movement Settings")]
    public float bobAmount = 0.5f;
    public float bobSpeed = 2f;
    public float moveSpeed = 3f;

    [Header("Natural Movement")]
    public float orbitSpeed = 20f;
    public float baseOrbitRadius = 12f;

    public float radiusPulseAmount = 1f;
    public float radiusPulseSpeed = 0.5f;

    public float orbitWobbleAmount = 0.4f;
    public float orbitWobbleSpeed = 1.2f;

    public float driftAmount = 1.5f;
    public float driftSpeed = 0.5f;

    public float microDriftAmount = 0.3f;
    public float microDriftSpeed = 3f;

    [Header("Laser Settings")]
    public float lockOnTime = 3f;
    public float firingTime = 3f;
    public float cooldownTime = 5f;
    public float laserRange = 400f;

    [Header("Difficulty Scaling")]
    public float chargeSpeedMultiplier = 1f;
    public float chargeSpeedRamp = 0.15f;

    [Header("Enemy Health")]
    public int maxHealth = 50;
    public int currentHealth;
    public EnemyHealthBar healthBar;

    private Vector3 frozenTargetPoint;
    private float chargeBeamStartWidth = 0.05f;
    private float chargeBeamEndWidth = 0.35f;

    private float orbitAngle;
    private float noiseOffsetX;
    private float noiseOffsetZ;

    private enum State { Tracking, Firing, Cooldown }
    private State currentState = State.Tracking;

    private void Start()
    {
        if (platformCenter == null)
            platformCenter = transform;

        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);

        StyleLaser(aimingLine, 0.08f, Color.yellow);
        StyleLaser(laserBeam, 0.35f, Color.red);

        chargeBeam.material = new Material(Shader.Find("Unlit/Color"));
        chargeBeam.material.color = Color.yellow;

        chargeBeam.startWidth = chargeBeamStartWidth;
        chargeBeam.endWidth = chargeBeamStartWidth;

        chargeBeam.enabled = false;
        laserBeam.enabled = false;

        if (chargeSound != null) chargeSound.loop = true;
        if (fireSound != null) fireSound.loop = true;

        if (screenFlash != null) screenFlash.alpha = 0;

        currentHealth = maxHealth;
        if (healthBar != null) healthBar.SetMax(maxHealth);

        StartCoroutine(StateMachine());
    }

    private void StyleLaser(LineRenderer lr, float width, Color color)
    {
        lr.startWidth = width;
        lr.endWidth = width;

        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            yield return currentState switch
            {
                State.Tracking => TrackingPhase(),
                State.Firing => FiringPhase(),
                State.Cooldown => CooldownPhase(),
                _ => null
            };
        }
    }

    // ---------------- TRACKING ----------------
    private IEnumerator TrackingPhase()
    {
        aimingLine.enabled = true;
        chargeBeam.enabled = true;

        float timer = 0f;

        if (chargeParticles != null) chargeParticles.Play();
        if (chargeSound != null) chargeSound.Play();

        RaycastHit hit;
        if (Physics.Raycast(player.position + Vector3.up, Vector3.down, out hit, 10f))
            frozenTargetPoint = hit.point;
        else
            frozenTargetPoint = player.position;

        float scaledLockOn = lockOnTime / chargeSpeedMultiplier;

        while (timer < scaledLockOn)
        {
            float t = timer / scaledLockOn;

            Vector3 origin = transform.position;
            Vector3 dir = (frozenTargetPoint - origin).normalized;

            chargeBeam.SetPosition(0, origin);
            chargeBeam.SetPosition(1, origin + dir * laserRange);

            float width = Mathf.Lerp(chargeBeamStartWidth, chargeBeamEndWidth, t);
            chargeBeam.startWidth = width;
            chargeBeam.endWidth = width;

            Color c = Color.Lerp(Color.yellow, Color.red, t);
            chargeBeam.material.color = c;
            chargeBeam.startColor = c;
            chargeBeam.endColor = c;

            timer += Time.deltaTime;
            yield return null;
        }

        currentState = State.Firing;
    }

    // ---------------- FIRING ----------------
    private IEnumerator FiringPhase()
    {
        aimingLine.enabled = false;
        chargeBeam.enabled = false;
        laserBeam.enabled = true;

        if (chargeParticles != null) chargeParticles.Stop();
        if (chargeSound != null) chargeSound.Stop();

        if (fireSound != null) fireSound.Play();

        if (screenFlash != null)
            StartCoroutine(ScreenFlashEffect());

        float timer = 0f;

        while (timer < firingTime)
        {
            Vector3 origin = transform.position;
            Vector3 dir = (frozenTargetPoint - origin).normalized;

            laserBeam.SetPosition(0, origin);
            laserBeam.SetPosition(1, origin + dir * laserRange);

            RaycastHit[] hits = Physics.SphereCastAll(origin, 0.5f, dir, laserRange);

            foreach (RaycastHit h in hits)
            {
                // ⭐ Correct player kill logic
                if (h.collider.CompareTag("Player"))
{
    FirstPersonController player = h.collider.GetComponentInParent<FirstPersonController>();
    if (player != null)
        player.PlayerDie();
}


                CubeHealth cube = h.collider.GetComponent<CubeHealth>();
                if (cube != null)
                    cube.TakeHit(1, true);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        laserBeam.enabled = false;

        if (fireSound != null) fireSound.Stop();

        currentState = State.Cooldown;
    }

    private IEnumerator ScreenFlashEffect()
    {
        float t = 0;
        while (t < 0.2f)
        {
            screenFlash.alpha = Mathf.Lerp(1, 0, t / 0.2f);
            t += Time.deltaTime;
            yield return null;
        }
        screenFlash.alpha = 0;
    }

    // ---------------- COOLDOWN ----------------
    private IEnumerator CooldownPhase()
    {
        float timer = 0f;

        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        chargeSpeedMultiplier += chargeSpeedRamp;

        currentState = State.Tracking;
    }

    // ---------------- HEALTH ----------------
    public void TakeDamage(int amount)
{
    currentHealth -= amount;
    if (healthBar != null) healthBar.SetValue(currentHealth);

    if (currentHealth <= 0)
    {
        FirstPersonController player = FindObjectOfType<FirstPersonController>();
        if (player != null)
            player.PlayVoice(player.vl_enemyKilled, player.freq_enemyKilled);

        Destroy(gameObject);
    }
}


    // ---------------- MOVEMENT ----------------
    private void Update()
    {
        Vector3 center = platformCenter.position;

        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        float pulsedRadius =
            baseOrbitRadius +
            Mathf.Sin(Time.time * radiusPulseSpeed) * radiusPulseAmount;

        Vector3 orbitPos = center + new Vector3(
            Mathf.Cos(rad) * pulsedRadius,
            0,
            Mathf.Sin(rad) * pulsedRadius
        );

        orbitPos.y += Mathf.Sin(Time.time * orbitWobbleSpeed) * orbitWobbleAmount;

        float driftX = (Mathf.PerlinNoise(Time.time * driftSpeed + noiseOffsetX, 0f) - 0.5f) * driftAmount;
        float driftZ = (Mathf.PerlinNoise(0f, Time.time * driftSpeed + noiseOffsetZ) - 0.5f) * driftAmount;

        float microX = (Mathf.PerlinNoise(Time.time * microDriftSpeed + noiseOffsetX, 99f) - 0.5f) * microDriftAmount;
        float microZ = (Mathf.PerlinNoise(99f, Time.time * microDriftSpeed + noiseOffsetZ) - 0.5f) * microDriftAmount;

        Vector3 drift = new Vector3(driftX + microX, 0, driftZ + microZ);

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        Vector3 targetPos = orbitPos + drift;
        targetPos.y = center.y + bob;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }
}
