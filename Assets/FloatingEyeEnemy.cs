using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingEyeEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public LineRenderer chargeBeam;
    public LineRenderer laserBeam;

    [Header("Effects")]
    public ParticleSystem chargeParticles;
    public AudioSource chargeSound;
    public AudioSource fireSound;
    public CanvasGroup screenFlash;

    [Header("Spaceship Movement")]
    public float orbitHeight = 8f;      // stays above player
    public float orbitRadius = 14f;     // stays away from player
    public float orbitSpeed = 12f;      // slow spaceship orbit
    public float bobAmount = 0.4f;
    public float bobSpeed = 1.5f;
    public float moveSpeed = 3f;        // FIXED: needed for Lerp

    [Header("Laser Settings")]
    public float lockOnTime = 3f;       // shrinks over time
    public float firingTime = 2f;
    public float cooldownTime = 3f;
    public float laserRange = 100f;

    public float laserTurnSpeed = 25f;      // turret rotation speed
    public float turnSpeedIncrease = 2f;    // difficulty ramp
    public float lockOnDecrease = 0.25f;    // difficulty ramp

    private Vector3 frozenTargetPoint;
    private Vector3 laserDir;
    private float orbitAngle;

    private enum State { Tracking, Firing, Cooldown }
    private State currentState = State.Tracking;

    private void Start()
    {
        // Auto-find player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Start laser pointing AWAY from player
        Vector3 initialDir = (player.position - transform.position).normalized;
        laserDir = Quaternion.Euler(0, -60f, 0) * initialDir;

        chargeBeam.enabled = false;
        laserBeam.enabled = false;

        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Tracking: yield return TrackingPhase(); break;
                case State.Firing: yield return FiringPhase(); break;
                case State.Cooldown: yield return CooldownPhase(); break;
            }
        }
    }

    // ---------------- TRACKING ----------------
    private IEnumerator TrackingPhase()
    {
        chargeBeam.enabled = true;

        float timer = 0f;

        if (chargeParticles != null) chargeParticles.Play();
        if (chargeSound != null) chargeSound.Play();

        // Freeze player position ONCE
        frozenTargetPoint = player.position;

        while (timer < lockOnTime)
        {
            // Slowly rotate laser toward frozen point
            Vector3 targetDir = (frozenTargetPoint - transform.position).normalized;

            Quaternion currentRot = Quaternion.LookRotation(laserDir);
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            Quaternion newRot = Quaternion.RotateTowards(
                currentRot,
                targetRot,
                laserTurnSpeed * Time.deltaTime
            );

            laserDir = newRot * Vector3.forward;

            // Update charge beam
            chargeBeam.SetPosition(0, transform.position);
            chargeBeam.SetPosition(1, transform.position + laserDir * laserRange);

            timer += Time.deltaTime;
            yield return null;
        }

        currentState = State.Firing;
    }

    // ---------------- FIRING ----------------
    private IEnumerator FiringPhase()
    {
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
            // Continue slow rotation toward frozen point
            Vector3 targetDir = (frozenTargetPoint - transform.position).normalized;

            Quaternion currentRot = Quaternion.LookRotation(laserDir);
            Quaternion targetRot = Quaternion.LookRotation(targetDir);

            Quaternion newRot = Quaternion.RotateTowards(
                currentRot,
                targetRot,
                laserTurnSpeed * Time.deltaTime
            );

            laserDir = newRot * Vector3.forward;

            // Update firing beam
            Vector3 origin = transform.position;
            laserBeam.SetPosition(0, origin);
            laserBeam.SetPosition(1, origin + laserDir * laserRange);

            // Damage
            RaycastHit[] hits = Physics.SphereCastAll(origin, 0.5f, laserDir, laserRange);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        laserBeam.enabled = false;

        // Difficulty ramp
        laserTurnSpeed += turnSpeedIncrease;
        lockOnTime = Mathf.Max(0.75f, lockOnTime - lockOnDecrease);

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
        yield return new WaitForSeconds(cooldownTime);
        currentState = State.Tracking;
    }

    // ---------------- MOVEMENT (SPACESHIP) ----------------
    private void Update()
    {
        if (player == null) return;

        // Orbit around player at a fixed height
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        Vector3 orbitPos = player.position + new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            orbitHeight,
            Mathf.Sin(rad) * orbitRadius
        );

        // Bobbing
        orbitPos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, orbitPos, Time.deltaTime * moveSpeed);
    }
}
