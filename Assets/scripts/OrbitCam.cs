using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // O jogador
    public Vector3 targetOffset = new Vector3(0, 1.6f, 0); // Altura dos olhos

    [Header("Camera Settings")]
    public float distance = 5f;
    public float sensitivity = 100f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.3f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitCamera precisa de um Target!");
            enabled = false;
            return;
        }

        // Inicia atrás do jogador
        yaw = target.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        // Pitch removido - câmera só roda horizontalmente
    }

    void UpdateCameraPosition()
    {
        Vector3 targetPosition = target.position + targetOffset;

        // Calcula a posição desejada da câmera (só horizontal)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        // Verifica colisões
        Vector3 direction = desiredPosition - targetPosition;
        RaycastHit hit;

        if (Physics.SphereCast(targetPosition, collisionRadius, direction.normalized, out hit, distance, collisionMask))
        {
            transform.position = targetPosition + direction.normalized * (hit.distance - collisionRadius);
        }
        else
        {
            transform.position = desiredPosition;
        }

        // Olha para o alvo
        transform.LookAt(targetPosition);
    }
}