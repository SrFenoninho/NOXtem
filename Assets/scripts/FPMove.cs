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
    public float jumpHeight = 1.2f;
    public float stickToGroundForce = 10f;      // forca para manter o jogador no chao em rampas
    public float gravityMultiplier = 3f;         // queda mais pesada e natural

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
    public bool useStuckDetection = true;       // deteta e recupera quando o jogador fica preso
    public float stuckThreshold = 0.01f;
    public float stuckTimeLimit = 2f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
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

    // Balanco da camera ao andar
    private float defaultCameraY;
    private float bobTimer = 0f;
    private float defaultSpeed; // Velocidade base do jogador

    private float nextFootstep = 0f;

    // Detecao de bloqueio na geometria
    private Vector3 previousPosition;
    private float stuckTimer = 0f;

    // bloqueado pelo IntroManager durante a intro
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

        defaultSpeed = speed; // Guardamos a velocidade original do jogador ao iniciar
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Som de aterragem
        if (isGrounded && !wasGrounded)
        {
            if (landSound != null) audioSource.PlayOneShot(landSound);
            isJumping = false;
        }

        // Input de salto guardado aqui, aplicado no FixedUpdate
        if (!inputBlocked && Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpInput = true;
            if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
        }

        // Balanco da cabeca ao andar
        if (useHeadBob && isGrounded)
        {
            bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
            HandleHeadBob(isMoving ? (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed) : 0);
        }

        // Aumento de FOV ao correr
        if (useFovKick)
        {
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && Input.GetAxisRaw("Vertical") > 0f;
            HandleFOVKick(isSprinting);
        }

        // Detecao de bloqueio na geometria
        if (useStuckDetection)
            HandleStuckDetection();
    }

    // ---------------------------------------------
    //  VISaO DO RATO
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
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && z > 0f;
        float currentSpeed = isSprinting ? sprintSpeed : speed;

        Vector3 inputDir = transform.right * x + transform.forward * z;
        if (inputDir.magnitude > 1f) inputDir.Normalize();

        moveDir.x = inputDir.x * currentSpeed;
        moveDir.z = inputDir.z * currentSpeed;

        if (isGrounded)
        {
            moveDir.y = -stickToGroundForce; // manter colado ao chao

            if (jumpInput)
            {
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpInput = false;
                isJumping = true;
            }
        }
        else
        {
            // Gravidade mais pesada no ar para queda mais natural
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        controller.Move(moveDir * Time.fixedDeltaTime);

        if (isGrounded && inputDir.magnitude > 0)
        {
            // Agora passamos a velocidade atual para os passos se ajustarem a ela
            HandleFootsteps(currentSpeed);
        }
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

            // Usamos exatamente a mesma suavização e rácio da câmara para manter a sincronização
            float rawMultiplier = defaultSpeed > 0 ? (currentSpeed / defaultSpeed) : 1f;
            float speedMultiplier = rawMultiplier > 1f ? Mathf.Lerp(1f, rawMultiplier, 0.5f) : rawMultiplier;

            // Dividimos o intervalo pelo multiplicador (mais veloz = menos tempo entre passos)
            float currentInterval = speedMultiplier > 0f ? (footstepInterval / speedMultiplier) : footstepInterval;

            nextFootstep = Time.time + currentInterval;
        }
    }

    // ---------------------------------------------
    //  DETEcaO DE BLOQUEIO
    // ---------------------------------------------
    // Deteta se o jogador esta preso na geometria e aplica impulso para libertar
    void HandleStuckDetection()
    {
        if (Vector3.Distance(previousPosition, transform.position) < stuckThreshold && !isGrounded)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeLimit)
            {
                moveDir.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
}
