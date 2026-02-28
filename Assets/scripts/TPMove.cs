using UnityEngine;

public class TPMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float gravityMultiplier = 2.5f;      // Heavier fall for more natural feel
    public float stickToGroundForce = 10f;      // Force to keep player grounded on slopes
    public float rotationSpeed = 10f;           // How fast the character turns

    [Header("Ground Check")]
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float footstepInterval = 0.4f;

    private CharacterController controller;
    private Vector3 moveDir;
    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpInput;
    private bool isJumping;
    private AudioSource audioSource;
    private float currentSpeed;
    private float nextFootstep = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Player");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Landing
        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            if (landSound != null)
                audioSource.PlayOneShot(landSound);
        }

        // Store jump input here, apply in FixedUpdate
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpInput = true;
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }
    }

    void FixedUpdate() // Physics and movement in FixedUpdate for consistency
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Get camera-relative movement direction
        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * v) + (right * h);

        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        // Rotate character to face movement direction
        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        moveDir.x = inputDir.x * currentSpeed;
        moveDir.z = inputDir.z * currentSpeed;

        if (isGrounded)
        {
            currentSpeed = isSprinting ? sprintSpeed : speed;
            moveDir.y = -stickToGroundForce; // Keep grounded on slopes

            if (jumpInput)
            {
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpInput = false;
                isJumping = true;
            }
        }
        else
        {
            // Heavier gravity while airborne
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        controller.Move(moveDir * Time.fixedDeltaTime);

        // Footsteps
        if (isGrounded && inputDir.magnitude > 0.1f)
            HandleFootsteps();
    }

    void HandleFootsteps()
    {
        if (Time.time >= nextFootstep && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
            nextFootstep = Time.time + footstepInterval;
        }
    }
}