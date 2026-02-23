using System.Collections;
using UnityEngine;

public class FPMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

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

    // Private variables
    CharacterController controller;
    Transform playerCamera;
    Camera cam;
    AudioSource audioSource;

    Vector3 velocity;
    float xRotation = 0f;
    float yRotation = 0f;
    bool isGrounded;
    bool wasGrounded;

    // Head bob
    private float defaultCameraY;
    private float bobTimer = 0f;

    // Footsteps
    private float nextFootstep = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.center = new Vector3(0, controller.height / 2, 0); // Ensure the center is at the correct height
        playerCamera = GetComponentInChildren<Camera>().transform;
        cam = playerCamera.GetComponent<Camera>();

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ground mask
        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Player");

        // Head bob setup
        defaultCameraY = playerCamera.localPosition.y;

        // FOV setup
        normalFOV = cam.fieldOfView;
    }

    void Update()
    {
        // Ground check
        RaycastHit hit;
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance + 0.1f, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Landing sound
        if (isGrounded && !wasGrounded && landSound != null)
        {
            audioSource.PlayOneShot(landSound);
        }

        // Movement
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded;
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Head bob
        if (useHeadBob && isGrounded)
        {
            HandleHeadBob(move.magnitude > 0 ? currentSpeed : 0);
        }

        // FOV kick (sprint)
        if (useFovKick)
        {
            HandleFOVKick(isSprinting);
        }

        // Footsteps
        if (isGrounded && move.magnitude > 0)
        {
            HandleFootsteps();
        }
    }

    void LateUpdate()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
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
            // Reset head bob when not moving
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

            nextFootstep = Time.time + footstepInterval;
        }
    }
}