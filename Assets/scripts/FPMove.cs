using System.Collections;
using UnityEngine;

public class FPMove : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float stickToGroundForce = 10f;
    public float gravityMultiplier = 3f;

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

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float crouchCameraOffset = 0.3f;
    public float crouchSpeed = 3f;
    public float crouchTransitionSpeed = 8f;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip landSound;
    public float footstepInterval = 0.5f;

    [Header("Stuck Detection")]
    public bool useStuckDetection = true;
    public float stuckThreshold = 0.01f;
    public float stuckTimeLimit = 2f;
    public float stuckRecoveryHeight = 1.2f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    CharacterController controller;
    Transform playerCamera;
    Camera cam;
    AudioSource audioSource;

    Vector3 moveDir;
    float xRotation = 0f;
    bool isGrounded;
    bool wasGrounded;

    private float defaultCameraY;
    private float bobTimer = 0f;
    private float defaultSpeed;

    private float nextFootstep = 0f;

    private Vector3 previousPosition;
    private float stuckTimer = 0f;

    private float standingHeight;
    [HideInInspector] public float standingCameraY;
    private float currentHeight;
    [HideInInspector] public bool isCrouching = false;

    [HideInInspector] public bool inputBlocked = false;
    [HideInInspector] public bool cameraBlocked = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
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
        defaultSpeed = speed;

        standingHeight = controller.height;
        standingCameraY = defaultCameraY;
        currentHeight = standingHeight;
    }

    void Update()
    {
        wasGrounded = isGrounded;

        Vector3 spherePos = new Vector3(transform.position.x,
                                        transform.position.y + controller.radius,
                                        transform.position.z);
        bool rayGrounded = Physics.CheckSphere(spherePos, controller.radius + groundDistance, groundMask);
        isGrounded = controller.isGrounded || rayGrounded;

        if (isGrounded && !wasGrounded)
        {
            if (landSound != null) audioSource.PlayOneShot(landSound);
        }

        HandleCrouch();

        if (useHeadBob && isGrounded)
        {
            bool isMoving = !inputBlocked && (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0);
            HandleHeadBob(isMoving ? (Input.GetKey(KeyCode.LeftShift) && !isCrouching ? sprintSpeed : speed) : 0);
        }

        if (useFovKick)
        {
            bool isSprinting = !inputBlocked && Input.GetKey(KeyCode.LeftShift) && !isCrouching && isGrounded && Input.GetAxisRaw("Vertical") > 0f;
            HandleFOVKick(isSprinting);
        }

        if (useStuckDetection)
            HandleStuckDetection();
    }

    // ---------------------------------------------
    //  VISAO DA CAMARA
    // ---------------------------------------------
    void LateUpdate()
    {
        if (inputBlocked || cameraBlocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 70f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ---------------------------------------------
    //  MOVIMENTO
    // ---------------------------------------------
    void FixedUpdate()
    {
        if (inputBlocked) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && isGrounded && z > 0f;
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : speed);

        Vector3 inputDir = transform.right * x + transform.forward * z;
        if (inputDir.magnitude > 1f) inputDir.Normalize();

        moveDir.x = inputDir.x * currentSpeed;
        moveDir.z = inputDir.z * currentSpeed;

        if (isGrounded)
        {
            moveDir.y = -stickToGroundForce;
        }
        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        controller.Move(moveDir * Time.fixedDeltaTime);

        if (isGrounded && inputDir.magnitude > 0)
            HandleFootsteps(currentSpeed);
    }

    // ---------------------------------------------
    //  EFEITOS VISUAIS / AUDIO
    // ---------------------------------------------
    void HandleHeadBob(float currentSpeed)
    {
        if (currentSpeed > 0)
        {
            float rawMultiplier = defaultSpeed > 0 ? (currentSpeed / defaultSpeed) : 1f;
            float speedMultiplier = rawMultiplier > 1f ? Mathf.Lerp(1f, rawMultiplier, 0.5f) : rawMultiplier;

            bobTimer += Time.deltaTime * (bobSpeed * speedMultiplier);
            float bobOffsetY = Mathf.Sin(bobTimer) * (bobAmount * speedMultiplier);

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

    void HandleFootsteps(float currentSpeed)
    {
        if (Time.time >= nextFootstep && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);

            float rawMultiplier = defaultSpeed > 0 ? (currentSpeed / defaultSpeed) : 1f;
            float speedMultiplier = rawMultiplier > 1f ? Mathf.Lerp(1f, rawMultiplier, 0.5f) : rawMultiplier;
            float currentInterval = speedMultiplier > 0f ? (footstepInterval / speedMultiplier) : footstepInterval;

            nextFootstep = Time.time + currentInterval;
        }
    }

    // ---------------------------------------------
    //  AGACHAMENTO
    // ---------------------------------------------
    void HandleCrouch()
    {
        if (inputBlocked) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isCrouching)
            {
                if (!CanStandUp()) return;
                isCrouching = false;
            }
            else
            {
                isCrouching = true;
            }
        }

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCamY   = isCrouching ? (standingCameraY - crouchCameraOffset) : standingCameraY;

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2f, 0);

        float camSpeed = crouchCameraOffset * crouchTransitionSpeed;
        defaultCameraY = Mathf.MoveTowards(defaultCameraY, targetCamY, Time.deltaTime * camSpeed);
    }

    bool CanStandUp()
    {
        float checkDistance = standingHeight - crouchHeight;
        Vector3 origin = transform.position + Vector3.up * crouchHeight;
        return !Physics.Raycast(origin, Vector3.up, checkDistance, groundMask);
    }

    // ---------------------------------------------
    //  DETECAO DE BLOQUEIO
    // ---------------------------------------------
    void HandleStuckDetection()
    {
        if (Vector3.Distance(previousPosition, transform.position) < stuckThreshold && !isGrounded)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeLimit)
            {
                moveDir.y = Mathf.Sqrt(stuckRecoveryHeight * -2f * gravity);
                stuckTimer = 0f;
                Debug.Log("Jogador preso - a tentar recuperar!");
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        previousPosition = transform.position;
    }

    public void SyncCameraRotation()
    {
        xRotation = playerCamera.localEulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f;
        xRotation = Mathf.Clamp(xRotation, -70f, 70f);
    }

    public float CurrentCameraY => defaultCameraY;
}
