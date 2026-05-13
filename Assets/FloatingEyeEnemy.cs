using System.Collections;
using UnityEngine;

public class FloatingEyeEnemy : MonoBehaviour
{
    [Header("References")]
    public GameObject player; // ⭐ MUST be the Player GameObject
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
    public float laserRange = 100f;

    private Vector3 patrolCenter;
    private Vector3 frozenTargetPoint;

    private Material eyeMat;
    private Vector3 originalPos;

    private float chargeBeamStartWidth = 0.05f;
    private float chargeBeamEndWidth = 0.35f;

    private float orbitAngle;
    private float noiseOffsetX;
    private float noiseOffsetZ;

    private enum State { Tracking, Firing, Cooldown }
    private State currentState = State.Tracking;

    // ⭐ Player controller reference for voice lines
    private FirstPersonController playerController;

    private void Start()
    {
        patrolCenter = transform.position;
        originalPos = transform.localPosition;

        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);

        // NON-GLOW LASERS
        StyleLaser(aimingLine, 0.08f, Color.yellow);
        StyleLaser(laserBeam, 0.35f, Color.red);
        StyleLaser(chargeBeam, chargeBeamStartWidth, Color.yellow);

        chargeBeam.enabled = false;

        eyeMat = GetComponentInChildren<Renderer>().material;

        if (chargeSound != null) chargeSound.loop = true;
        if (fireSound != null) fireSound.loop = false;

        if (screenFlash != null) screenFlash.alpha = 0;

        // ⭐ Assign player controller correctly
        if (player != null)
            playerController = player.GetComponent<FirstPersonController>();

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

    // ---------------- TRACKING (CHARGING) ----------------
    private IEnumerator TrackingPhase()
    {
        aimingLine.enabled = true;
        chargeBeam.enabled = true;

        float timer = 0f;

        if (chargeParticles != null) chargeParticles.Play();
        if (chargeSound != null) chargeSound.Play();

        // ⭐ Trigger player voice line + UI warning
        if (playerController != null)
        {
            Debug.Log("Enemy charging → calling ShowChargeWarning()");
            playerController.ShowChargeWarning();
        }

        frozenTargetPoint = player.transform.position;

        while (timer < lockOnTime)
        {
            float t = timer / lockOnTime;

            chargeBeam.SetPosition(0, transform.position);
            chargeBeam.SetPosition(1, frozenTargetPoint);

            float width = Mathf.Lerp(chargeBeamStartWidth, chargeBeamEndWidth, t);
            chargeBeam.startWidth = width;
            chargeBeam.endWidth = width;

            chargeBeam.material.color = Color.Lerp(Color.yellow, Color.red, t);

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

        // ⭐ Trigger firing voice line
        if (playerController != null)
        {
            Debug.Log("Enemy firing → calling PlayVoice()");
            playerController.PlayVoice(playerController.vl_enemyFiring, playerController.freq_enemyFiring);
        }

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

            foreach (RaycastHit hit in hits)
            {
                CubeHealth cube = hit.collider.GetComponent<CubeHealth>();
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

        currentState = State.Tracking;
    }

    // ---------------- MOVEMENT ----------------
    private void Update()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        float pulsedRadius =
            baseOrbitRadius +
            Mathf.Sin(Time.time * radiusPulseSpeed) * radiusPulseAmount;

        Vector3 orbitPos = patrolCenter + new Vector3(
            Mathf.Cos(rad) * pulsedRadius,
            0,
            Mathf.Sin(rad) * pulsedRadius
        );

        orbitPos.y += Mathf.Sin(Time.time * orbitWobbleSpeed) * orbitWobbleAmount;

        // ⭐ FIXED PerlinNoise Z-axis bug
        float driftX = (Mathf.PerlinNoise(Time.time * driftSpeed + noiseOffsetX, 0f) - 0.5f) * driftAmount;
        float driftZ = (Mathf.PerlinNoise(0f, Time.time * driftSpeed + noiseOffsetZ) - 0.5f) * driftAmount;

        float microX = (Mathf.PerlinNoise(Time.time * microDriftSpeed + noiseOffsetX, 99f) - 0.5f) * microDriftAmount;
        float microZ = (Mathf.PerlinNoise(99f, Time.time * microDriftSpeed + noiseOffsetZ) - 0.5f) * microDriftAmount;

        Vector3 drift = new Vector3(driftX + microX, 0, driftZ + microZ);

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        Vector3 targetPos = orbitPos + drift;
        targetPos.y = patrolCenter.y + bob;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }
}
