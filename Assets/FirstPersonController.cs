using UnityEngine;
using System.Collections;

public class FirstPersonController : MonoBehaviour
{
    CharacterController characterController;
    Camera cam;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float mouseSensitivity = 2f;
    public float UpDownRange = 80f;

    float verticalRotation = 0f;
    Vector3 movement;
    float currentSpeed;

    bool wasGrounded = false;
public int extraJumps = 0;

    [Header("Special Attack")]
    public bool hasSpecialAttack = false;

    [Header("Projectile Special Attack")]
    public GameObject projectilePrefab;          // sphere projectile
    public Transform projectileSpawnPoint;       // empty object in front of camera
    public float projectileSpeed = 40f;          // adjustable speed

    [Header("Laser (unused now)")]
    public LineRenderer playerLaser;
    public float laserDuration = 0.1f;
    public float laserMaxDistance = 100f;

    [Header("UI + Effects")]
    public CanvasGroup specialFlash;
    public CanvasGroup chargeWarningUI;
    public float flashFadeSpeed = 4f;
    public float chargeWarningFadeSpeed = 2f;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip walkStep;
    public AudioClip sprintStep;
    public AudioClip jumpLine;
    public AudioClip pickupLine;
    public AudioClip vl_specialCubeDestroyed;
    public AudioClip specialReadyLine;
    public AudioClip vl_specialAttackUsed;
    public AudioClip vl_laserMissed;
    public AudioClip vl_enemyCharging;
    public AudioClip vl_enemyKilled;

    public float freq_specialCubeDestroyed = 1f;
    public float freq_specialAttackUsed = 1f;
    public float freq_laserMissed = 1f;
    public float freq_enemyCharging = 1f;
    public float freq_enemyKilled = 1f;

    public AudioClip vl_cubeDestroyed;
public float freq_cubeDestroyed = 1f;


    [Header("Raycast")]
    public float interactDistance = 5f;
    Vector3 hitPoint;

    [Header("Particles")]
    public ParticleSystem impactPS;
    public int particleCount = 10;

    

    public void PlayerDie()
{
    // Optional: play a death sound
    PlayVoice(vl_enemyKilled, 1f);

    // Restart the scene
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
    );
}


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

                Debug.Log("Player spawned at: " + transform.position);

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
    HandleLandingImpact();
    HandleFootsteps();
    HandleChargeWarningUI();

    // ⭐ ONLY THIS — ONE FIRING BLOCK
    if (Input.GetKeyDown(KeyCode.F) && hasSpecialAttack)
{
    GameObject proj = Instantiate(
        projectilePrefab,
        projectileSpawnPoint.position,
        projectileSpawnPoint.rotation
    );

    PlayerProjectile p = proj.GetComponent<PlayerProjectile>();
    if (p != null)
        p.shootDirection = cam.transform.forward;

    hasSpecialAttack = false;   // ⭐ consume the special attack
    PlayVoice(vl_specialAttackUsed, freq_specialAttackUsed);
}

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
        if (!wasGrounded && characterController.isGrounded)
        {
            cam.transform.localPosition = new Vector3(0, -0.1f, 0);
        }
    }

    // ---------------------------------------------------------
    // FOOTSTEPS
    // ---------------------------------------------------------

    float stepTimer = 0f;
    public float stepInterval = 0.5f;
    public bool useFootsteps = true;

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
    // CHARGE WARNING UI
    // ---------------------------------------------------------

    public bool useChargeWarning = true;

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

    public bool useSpecialFlash = true;

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
    // ⭐ SPECIAL ATTACK — PROJECTILE VERSION
    // ---------------------------------------------------------

    void HandleSpecialAttack()
    {
        if (!Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.F))
            return;

        if (!hasSpecialAttack)
            return;

        TriggerSpecialFlash();

        // ⭐ Spawn projectile
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject proj = Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                projectileSpawnPoint.rotation
            );

            PlayerProjectile p = proj.GetComponent<PlayerProjectile>();
            if (p != null)
                p.speed = projectileSpeed;
        }

        hasSpecialAttack = false;

        PlayVoice(vl_specialAttackUsed, freq_specialAttackUsed);
    }

    // ---------------------------------------------------------
    // DAMAGE / DESTROY
    // ---------------------------------------------------------

    void HandleDestroy()
{
    GameObject obj = ObjectInFocus();
    if (obj == null) return;

    //Debug.Log("Hit: " + obj.name + " | Tag: " + obj.tag);//


    // ⭐ Make platform indestructible
    if (obj.CompareTag("Platform"))
        return;

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
            return;
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

    // ---------------------------------------------------------
    // AUDIO HELPER
    // ---------------------------------------------------------

public void PlayVoice(AudioClip clip, float volume)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
    }
}
