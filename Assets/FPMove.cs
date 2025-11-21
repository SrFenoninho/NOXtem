using UnityEngine;

public class FPMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("Ground Check")] //verifica se está no chão
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;

    CharacterController controller;
    Transform playerCamera;
    Vector3 velocity;
    float xRotation = 0f;
    float yRotation = 0f;
    bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (groundMask == 0)
        {
            groundMask = ~LayerMask.GetMask("Player");
        }
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance + 0.1f, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movimentos básicos
        float x = Input.GetAxis("Horizontal");   // A / D
        float z = Input.GetAxis("Vertical");     // W / S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Fórmula matemática para calcular a altura do pulo

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}