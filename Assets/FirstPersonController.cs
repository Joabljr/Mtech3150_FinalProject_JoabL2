using UnityEngine;
using System.Collections;

public class FirstPersonController : MonoBehaviour
{
    private CharacterController characterController;
    public Camera cam;

    private bool hasSpecialAttack = false;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = 9.81f;

    public int extraJumps = 0;

    private float currentSpeed;
    private Vector3 movement;
    private float verticalRotation;
    public float mouseSensitivity = 2f;
    public float UpDownRange = 60f;

    [Header("Raycast / Interaction")]
    private Vector3 hitPoint;
    public float interactDistance = 5f;

    [Header("Particles")]
    public ParticleSystem impactPS;
    public int particleCount = 20;

    [Header("Special Attack Laser")]
    public LineRenderer playerLaser;
    public float laserDuration = 0.15f;
    public float laserMaxDistance = 50f;

    // ---------------------------------------------------------
    // ⭐ FUN FEATURES YOU KEPT
    // ---------------------------------------------------------

    [Header("Landing Impact")]
    public bool useLandingImpact = true;
    public float landingKick = 4f;
    private bool wasGrounded;

    [Header("Footstep Sounds")]
    public bool useFootsteps = true;
    public AudioSource footstepSource;
    public AudioClip walkStep;
    public AudioClip sprintStep;
    public float stepInterval = 0.45f;
    private float stepTimer;

    [Header("Special Attack Flash")]
    public bool useSpecialFlash = true;
    public CanvasGroup specialFlash;
    public float flashFadeSpeed = 6f;

    [Header("Enemy Charging Warning")]
    public bool useChargeWarning = true;
    public CanvasGroup chargeWarningUI;
    public float chargeWarningFadeSpeed = 3f;

    // ---------------------------------------------------------
    // ⭐ PLAYER VOICE LINES (existing)
    // ---------------------------------------------------------

    [Header("Player Voice Lines")]
    public bool useVoice = true;
    public AudioSource voiceSource;
    public AudioClip pickupLine;
    public AudioClip specialReadyLine;
    public AudioClip jumpLine;

    // ---------------------------------------------------------
    // ⭐ NEW — GENERIC VOICE LINES YOU CAN USE NOW
    // ---------------------------------------------------------

    [Header("Generic Voice Lines — NOW")]
    public AudioClip vl_specialAttackUsed;
    public AudioClip vl_enemyCharging;
    public AudioClip vl_enemyFiring;
    public AudioClip vl_cubeDestroyed;
    public AudioClip vl_specialCubeDestroyed;
    public AudioClip vl_lowHealth;
    public AudioClip vl_laserMissed;

    // ---------------------------------------------------------
    // ⭐ NEW — GENERIC VOICE LINES YOU WILL USE LATER
    // ---------------------------------------------------------

    [Header("Generic Voice Lines — LATER")]
    public AudioClip vl_playerDamaged;
    public AudioClip vl_playerDeath;
    public AudioClip vl_enemySpawned;
    public AudioClip vl_enemyKilled;
    public AudioClip vl_newArea;
    public AudioClip vl_bossApproaching;
    public AudioClip vl_tutorialHint;

    // ---------------------------------------------------------
    // ⭐ NEW — GENERIC VOICE LINES YOU SHOULD HAVE (FUTURE PROOF)
    // ---------------------------------------------------------

    [Header("Generic Voice Lines — SHOULD HAVE")]
    public AudioClip vl_itemCollected;
    public AudioClip vl_upgradeInstalled;
    public AudioClip vl_worldEvent;
    public AudioClip vl_secretFound;
    public AudioClip vl_glitch01;
    public AudioClip vl_glitch02;
    public AudioClip vl_glitch03;

    // ---------------------------------------------------------
    // ⭐ VOICE FREQUENCY CONTROLS
    // ---------------------------------------------------------

    [Header("Voice Line Frequency Controls")]
    [Range(0f, 1f)] public float freq_enemyCharging = 1f;
    [Range(0f, 1f)] public float freq_enemyFiring = 1f;
    [Range(0f, 1f)] public float freq_cubeDestroyed = 1f;
    [Range(0f, 1f)] public float freq_specialCubeDestroyed = 1f;
    [Range(0f, 1f)] public float freq_specialAttackUsed = 1f;
    [Range(0f, 1f)] public float freq_lowHealth = 1f;
    [Range(0f, 1f)] public float freq_laserMissed = 1f;
    [Range(0f, 1f)] public float freq_playerDamaged = 1f;
    [Range(0f, 1f)] public float freq_playerDeath = 1f;
    [Range(0f, 1f)] public float freq_enemySpawned = 1f;
    [Range(0f, 1f)] public float freq_enemyKilled = 1f;
    [Range(0f, 1f)] public float freq_newArea = 1f;
    [Range(0f, 1f)] public float freq_bossApproaching = 1f;
    [Range(0f, 1f)] public float freq_itemCollected = 1f;
    [Range(0f, 1f)] public float freq_upgradeInstalled = 1f;
    [Range(0f, 1f)] public float freq_worldEvent = 1f;
    [Range(0f, 1f)] public float freq_secretFound = 1f;

    // ---------------------------------------------------------
    // ⭐ VOICE HELPER FUNCTION
    // ---------------------------------------------------------

    public void PlayVoice(AudioClip clip, float frequency)
    {
        if (!useVoice || voiceSource == null || clip == null)
            return;

        if (Random.value <= frequency)
            voiceSource.PlayOneShot(clip);
    }

    // ---------------------------------------------------------

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerLaser != null)
            playerLaser.enabled = false;

        if (specialFlash != null)
            specialFlash.alpha = 0;

        if (chargeWarningUI != null)
            chargeWarningUI.alpha = 0;
    }

    // ⭐ Player receives special attack from purple cubes
    public void GainSpecialAttack()
    {
        if (hasSpecialAttack)
            return;

        hasSpecialAttack = true;

        PlayVoice(vl_specialCubeDestroyed, freq_specialCubeDestroyed);
        PlayVoice(specialReadyLine, 1f);
    }

    // ⭐ Player receives extra jump from special cubes
    public void GainExtraJump()
    {
        extraJumps += 1;
        PlayVoice(pickupLine, 1f);
    }

    void Update()
    {
        MouseLook();
        Movement();
        Jumping();
        Sprinting();

        HandleDestroy();
        HandleSpecialAttack();

        HandleLandingImpact();
        HandleFootsteps();
        HandleChargeWarningUI();

        if (Input.GetKeyDown(KeyCode.T))
            PlayVoice(vl_enemyCharging, 1f);
    }

    // ---------------------------------------------------------
    // MOVEMENT
    // ---------------------------------------------------------

    void Movement()
    {
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        Vector3 move = transform.right * hor + transform.forward * ver;
        movement.x = move.x * currentSpeed;
        movement.z = move.z * currentSpeed;

        characterController.Move(movement * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -UpDownRange, UpDownRange);

        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void Sprinting()
    {
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
    }

    // ---------------------------------------------------------
    // ⭐ SIMPLE JUMP
    // ---------------------------------------------------------

    void Jumping()
    {
        bool grounded = characterController.isGrounded;

        if (grounded)
        {
            movement.y = -1f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.y = jumpForce;
                PlayVoice(jumpLine, 1f);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 0)
            {
                movement.y = jumpForce;
                extraJumps--;
            }

            movement.y -= gravity * Time.deltaTime;
        }

        wasGrounded = grounded;
    }

    // ---------------------------------------------------------
    // LANDING IMPACT
    // ---------------------------------------------------------

    void HandleLandingImpact()
    {
        if (!useLandingImpact) return;

        if (!wasGrounded && characterController.isGrounded)
        {
            cam.transform.localPosition = new Vector3(0, -0.1f, 0);
        }
    }

    // ---------------------------------------------------------
    // FOOTSTEPS
    // ---------------------------------------------------------

    void HandleFootsteps()
    {
        if (!useFootsteps || footstepSource == null) return;

        if (characterController.isGrounded && characterController.velocity.magnitude > 0.2f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                stepTimer = 0;

                AudioClip clip = currentSpeed == sprintSpeed ? sprintStep : walkStep;
                if (clip != null)
                    footstepSource.PlayOneShot(clip);
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    // ---------------------------------------------------------
    // CHARGE WARNING UI (enemy charging)
    // ---------------------------------------------------------

    public void ShowChargeWarning()
    {
        if (!useChargeWarning || chargeWarningUI == null) return;

        chargeWarningUI.alpha = 1;

        PlayVoice(vl_enemyCharging, freq_enemyCharging);
    }

    void HandleChargeWarningUI()
    {
        if (!useChargeWarning || chargeWarningUI == null) return;

        chargeWarningUI.alpha = Mathf.Lerp(chargeWarningUI.alpha, 0, Time.deltaTime * chargeWarningFadeSpeed);
    }

    // ---------------------------------------------------------
    // SPECIAL ATTACK FLASH
    // ---------------------------------------------------------

    void TriggerSpecialFlash()
    {
        if (!useSpecialFlash || specialFlash == null) return;
        StartCoroutine(SpecialFlashRoutine());
    }

    IEnumerator SpecialFlashRoutine()
    {
        specialFlash.alpha = 1;
        while (specialFlash.alpha > 0)
        {
            specialFlash.alpha -= Time.deltaTime * flashFadeSpeed;
            yield return null;
        }
    }

    // ---------------------------------------------------------
    // SPECIAL ATTACK
    // ---------------------------------------------------------

    IEnumerator FirePlayerLaser(Vector3 start, Vector3 end)
    {
        if (playerLaser == null)
            yield break;

        playerLaser.positionCount = 2;
        playerLaser.SetPosition(0, start);
        playerLaser.SetPosition(1, end);

        playerLaser.widthMultiplier = 0.15f;

        playerLaser.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        playerLaser.enabled = false;
    }

    void HandleSpecialAttack()
    {
        if (!Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.F))
            return;

        if (!hasSpecialAttack)
        {
            // No special attack available — no voice line needed
            return;
        }

        TriggerSpecialFlash();

        RaycastHit hit;
        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;
        Vector3 laserEndPoint = origin + direction * laserMaxDistance;

        bool hitSomething = false;

        if (Physics.Raycast(origin, direction, out hit, laserMaxDistance))
        {
            hitSomething = true;
            laserEndPoint = hit.point;

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeSpecialDamage(1);
                hasSpecialAttack = false;
            }
        }

        Vector3 start = cam.transform.position + cam.transform.forward * 1f;
        StartCoroutine(FirePlayerLaser(start, laserEndPoint));

        PlayVoice(vl_specialAttackUsed, freq_specialAttackUsed);

        if (!hitSomething)
            PlayVoice(vl_laserMissed, freq_laserMissed);
    }

    // ---------------------------------------------------------
    // DAMAGE / DESTROY
    // ---------------------------------------------------------

    void HandleDestroy()
    {
        GameObject obj = ObjectInFocus();
        if (obj == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (impactPS != null)
            {
                impactPS.transform.position = hitPoint;
                impactPS.Emit(particleCount);
            }

            CubeHealth hp = obj.GetComponent<CubeHealth>();
            if (hp != null)
            {
                hp.TakeHit(1);
                return; // ⭐ cubeDestroyed voice removed (CubeHealth handles it)
            }

            EnemyHealth enemy = obj.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                PlayVoice(vl_enemyKilled, freq_enemyKilled);
                return;
            }

            Destroy(obj);
        }
    }

    // ---------------------------------------------------------
    // RAYCAST
    // ---------------------------------------------------------

    GameObject ObjectInFocus()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, interactDistance))
        {
            hitPoint = hit.point;
            return hit.collider.gameObject;
        }

        return null;
    }
}
