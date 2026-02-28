using System.Collections;
using UnityEngine;

public class FPMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float stickToGroundForce = 10f;      // Force to keep player grounded on slopes
    public float gravityMultiplier = 3f;         // Multiplier for heavier fall gravity

    [Header("Ground Check")]
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;

    [Header("Head Bob")]
    public bool useHeadBob = true;
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    [Header("FOV Kick")]
    public bool useFovKick = true;
    public float sprintFOV = 75f;
    private float normalFOV = 60f;
    public float fovSmoothSpeed = 5f;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float footstepInterval = 0.5f;

    [Header("Stuck Detection")]
    public bool useStuckDetection = true;       // Detects and recovers when player gets stuck
    public float stuckThreshold = 0.01f;
    public float stuckTimeLimit = 2f;

    // Private variables
    CharacterController controller;
    Transform playerCamera;
    Camera cam;
    AudioSource audioSource;

    Vector3 velocity;
    Vector3 moveDir;
    float xRotation = 0f;
    bool isGrounded;
    bool wasGrounded;
    bool isJumping;
    bool jumpInput;

    // Head bob
    private float defaultCameraY;
    private float bobTimer = 0f;

    // Footsteps
    private float nextFootstep = 0f;

    // Stuck detection
    private Vector3 previousPosition;
    private float stuckTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.center = new Vector3(0, controller.height / 2, 0);
        playerCamera = GetComponentInChildren<Camera>().transform;
        cam = playerCamera.GetComponent<Camera>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Player");

        defaultCameraY = playerCamera.localPosition.y;
        normalFOV = cam.fieldOfView;
        previousPosition = transform.position;
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded; // Use CharacterController built-in ground check

        // Landing sound
        if (isGrounded && !wasGrounded)
        {
            if (landSound != null)
                audioSource.PlayOneShot(landSound);

            isJumping = false;
        }

        // Jump input stored here, applied in FixedUpdate for physics consistency
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpInput = true;
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        // Head bob
        if (useHeadBob && isGrounded)
        {
            bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
            HandleHeadBob(isMoving ? (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed) : 0);
        }

        // FOV kick
        if (useFovKick)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && Input.GetAxisRaw("Vertical") > 0f;
            HandleFOVKick(isSprinting);
        }

        // Stuck detection
        if (useStuckDetection)
            HandleStuckDetection();
    }

    void LateUpdate()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void FixedUpdate() // Physics handled in FixedUpdate for consistent framerate
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && z > 0f;
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        Vector3 inputDir = transform.right * x + transform.forward * z;
        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        moveDir.x = inputDir.x * currentSpeed;
        moveDir.z = inputDir.z * currentSpeed;

        if (isGrounded)
        {
            moveDir.y = -stickToGroundForce; // Keep player grounded on slopes

            if (jumpInput)
            {
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpInput = false;
                isJumping = true;
            }
        }
        else
        {
            // Apply heavier gravity while airborne for more natural fall
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        controller.Move(moveDir * Time.fixedDeltaTime);

        // Footsteps
        if (isGrounded && inputDir.magnitude > 0)
            HandleFootsteps();
    }

    void HandleHeadBob(float speed)
    {
        if (speed > 0)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;
            Vector3 newPos = playerCamera.localPosition;
            newPos.y = defaultCameraY + bobOffsetY;
            playerCamera.localPosition = newPos;
        }
        else
        {
            bobTimer = 0f;
            Vector3 newPos = playerCamera.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, defaultCameraY, Time.deltaTime * 5f);
            playerCamera.localPosition = newPos;
        }
    }

    void HandleFOVKick(bool isSprinting)
    {
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }

    void HandleFootsteps()
    {
        if (Time.time >= nextFootstep)
        {
            if (footstepSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, footstepSounds.Length);
                audioSource.PlayOneShot(footstepSounds[randomIndex]);
            }
            nextFootstep = Time.time + (Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0f ? footstepInterval * 0.6f : footstepInterval);
        }
    }

    // Detects if the player is stuck in geometry and attempts recovery
    void HandleStuckDetection()
    {
        if (Vector3.Distance(previousPosition, transform.position) < stuckThreshold && !isGrounded)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeLimit)
            {
                // Apply upward impulse to free the player
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                stuckTimer = 0f;
                Debug.Log("Player stuck - attempting recovery!");
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        previousPosition = transform.position;
    }
}