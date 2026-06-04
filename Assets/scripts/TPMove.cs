using UnityEngine;

public class TPMove : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;
    public float gravityMultiplier = 2.5f;
    public float stickToGroundForce = 10f;  // forca para manter o jogador no chao em rampas
    public float rotationSpeed = 10f;
    public float attackSpeedMultiplier = 1.0f; // Removido o limite! Podes meter 2 ou 3 para acelerar.

    [Header("Ground Check")]
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Animation")]
    public Animator anim;

    private float gravitySuspensionEndTime = 0f;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float footstepInterval = 0.4f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private CharacterController controller;
    private Vector3 moveDir;
    private bool isGrounded;
    private bool wasGrounded;
    private bool jumpInput;
    private bool isJumping;
    private AudioSource audioSource;
    private float currentSpeed;
    private float nextFootstep = 0f;

    // Impulso de ataque - aplicado separadamente do movimento normal
    private Vector3 attackImpulseDir;
    private float attackImpulseForce;
    private float attackImpulseEndTime;

    private PlayerCombat playerCombat;
    public bool IsGrounded => isGrounded; // exposto para PlayerCombat verificar se pode defender

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
        // Bloquear movimento enquanto defende ou carrega ataque pesado
        if (playerCombat != null && (playerCombat.IsDefending || playerCombat.IsHeavyCharging)) return;

        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Som de aterragem
        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
            if (landSound != null)
                audioSource.PlayOneShot(landSound);
        }

        // Input de salto guardado aqui, aplicado no FixedUpdate para consistencia fisica
        if (Input.GetButtonDown("Jump") && isGrounded && !isJumping)
        {
            jumpInput = true;
            if (jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }

        // Aplicar impulso de ataque diretamente, mesma abordagem que o knockback do PlayerHealth
        if (Time.time < attackImpulseEndTime)
        {
            controller.Move(attackImpulseDir * attackImpulseForce * Time.deltaTime);
        }
    }

    // ---------------------------------------------
    //  MOVIMENTO
    // ---------------------------------------------
    void FixedUpdate()
    {
        if (inputBlocked) return;
        if (playerCombat != null && (playerCombat.IsDefending || playerCombat.IsHeavyCharging)) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Calcular direcao relativa a camera
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

        // Rodar o jogador na direcao do movimento
        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (isGrounded)
        {
            currentSpeed = isSprinting ? sprintSpeed : speed;
            moveDir.y = -stickToGroundForce; // manter colado ao chao em rampas

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
                // God of War / DMC float effect (reduz muito a gravidade)
                moveDir += Physics.gravity * (gravityMultiplier * 0.1f) * Time.fixedDeltaTime;
                
                // Nao deixa o jogador continuar a subir se ja estava a descer, mantem no ar
                if (moveDir.y < 0) moveDir.y = Mathf.Max(moveDir.y, -1f);
            }
            else
            {
                // Aplicar gravidade mais pesada no ar para queda mais natural
                moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
            }
        }

        // Abranda o personagem se estiver a atacar no chao
        float activeSpeed = currentSpeed;
        if (playerCombat != null && playerCombat.IsAttacking && isGrounded)
        {
            activeSpeed *= attackSpeedMultiplier;
        }

        if (playerCombat != null && playerCombat.IsMovementLocked)
        {
            activeSpeed = 0f; // Corta a velocidade horizontal para 0
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
    //  PASSOS
    // ---------------------------------------------
    void HandleFootsteps()
    {
        if (Time.time >= nextFootstep && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
            // Intervalo mais curto a correr
            nextFootstep = Time.time + (Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0f
                ? footstepInterval * 0.6f
                : footstepInterval);
        }
    }

    public void SuspendGravity(float duration)
    {
        gravitySuspensionEndTime = Time.time + duration;
        
        // Se estava a subir (e.g. no inicio do salto), para de subir para nao voar infinitamente
        if (moveDir.y > 0) moveDir.y = 0f; 
        
        // Se estava a cair, para de cair instantaneamente
        if (moveDir.y < 0) moveDir.y = 0f; 
    }

    // ---------------------------------------------
    //  IMPULSO DE ATAQUE
    // ---------------------------------------------
    public void AddImpulse(Vector3 direction, float force)
    {
        attackImpulseDir = direction;
        attackImpulseForce = force;
        attackImpulseEndTime = Time.time + 0.15f;
    }
}
