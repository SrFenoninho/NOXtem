using UnityEngine;

public class TPMove : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float gravityMultiplier = 2.5f;
    public float stickToGroundForce = 10f;
    public float rotationSpeed = 10f;
    public float attackSpeedMultiplier = 1.0f;

    [Header("Ground Check")]
    public float groundDistance = 0.3f;

    public LayerMask groundMask;

    [Header("Animation")]
    public Animator anim;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private float gravitySuspensionEndTime = 0f;

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

    private Vector3 attackImpulseDir;
    private float attackImpulseForce;
    private float attackImpulseEndTime;

    private PlayerCombat playerCombat;
    public bool IsGrounded => isGrounded;

    [HideInInspector] public bool inputBlocked = false;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        controller = GetComponent<CharacterController>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Player");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (inputBlocked) return;
        if (playerCombat != null && (playerCombat.IsDefending || playerCombat.IsHeavyCharging)) return;

        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            if (landSound != null)
                audioSource.PlayOneShot(landSound);
        }

        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpInput = true;
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        if (Time.time < attackImpulseEndTime)
        {
            controller.Move(attackImpulseDir * attackImpulseForce * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (inputBlocked) return;
        if (playerCombat != null && (playerCombat.IsDefending || playerCombat.IsHeavyCharging)) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

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

        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (isGrounded)
        {
            currentSpeed = isSprinting ? sprintSpeed : speed;
            moveDir.y = -stickToGroundForce;

            if (jumpInput)
            {
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpInput = false;
                isJumping = true;
                if (anim != null) anim.SetTrigger("doJump");
            }
        }
        else
        {
            if (Time.time < gravitySuspensionEndTime)
            {
                moveDir += Physics.gravity * (gravityMultiplier * 0.1f) * Time.fixedDeltaTime;

                if (moveDir.y < 0) moveDir.y = Mathf.Max(moveDir.y, -1f);
            }
            else
            {
                moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
            }
        }

        float activeSpeed = currentSpeed;
        if (playerCombat != null && playerCombat.IsAttacking && isGrounded)
        {
            activeSpeed *= attackSpeedMultiplier;
        }

        if (playerCombat != null && playerCombat.IsMovementLocked)
        {
            activeSpeed = 0f;
        }

        moveDir.x = inputDir.x * activeSpeed;
        moveDir.z = inputDir.z * activeSpeed;

        if (anim != null)
        {
            anim.SetBool("isGrounded", isGrounded);
            float animSpeed = inputDir.magnitude * currentSpeed;
            anim.SetFloat("Speed", animSpeed);
        }

        controller.Move(moveDir * Time.fixedDeltaTime);

        if (isGrounded && inputDir.magnitude > 0.1f)
            HandleFootsteps();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void HandleFootsteps()
    {
        if (Time.time >= nextFootstep && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
            nextFootstep = Time.time + (Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0f
                ? footstepInterval * 0.6f
                : footstepInterval);
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void SuspendGravity(float duration)
    {
        gravitySuspensionEndTime = Time.time + duration;

        if (moveDir.y > 0) moveDir.y = 0f; 

        if (moveDir.y < 0) moveDir.y = 0f; 
    }

    public void AddImpulse(Vector3 direction, float force)
    {
        attackImpulseDir = direction;
        attackImpulseForce = force;
        attackImpulseEndTime = Time.time + 0.15f;
    }
}
