using UnityEngine;

public class TPMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("Ground Check")]
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    CharacterController controller;
    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance + 0.1f, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        // Direção corrigida, sem inversões
        Vector3 moveDirection = (transform.forward * v) + (transform.right * h);

        // Movimento
        if (moveDirection.magnitude > 0.1f)
        {
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);

            // Rotação do modelo na direção do movimento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // Gravidade
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}
