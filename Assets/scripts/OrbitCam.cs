using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Target")]
    public Transform target;                            // o jogador
    public Vector3 targetOffset = new Vector3(0, 1.6f, 0); // altura dos olhos

    [Header("Camera Settings")]
    public float distance = 5f;
    public float sensitivity = 100f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.3f;                // raio do SphereCast para evitar penetracao

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float yaw = 0f;     // rotacao horizontal
    private float pitch = 20f;  // rotacao vertical (fixa - so roda horizontalmente)

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (target == null)
        {
            // Debug.LogError("OrbitCamera precisa de um Target!");
            enabled = false;
            return;
        }

        // Iniciar atras do jogador
        yaw = target.eulerAngles.y;

        sensitivity = PlayerPrefs.GetFloat("Sensitivity", 100f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    // ---------------------------------------------
    //  INPUT
    // ---------------------------------------------
    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * (sensitivity * 0.05f);
        yaw += mouseX;
        // Pitch removido - camera so roda horizontalmente (design intencional)
    }

    // ---------------------------------------------
    //  POSICIONAMENTO
    // ---------------------------------------------
    void UpdateCameraPosition()
    {
        Vector3 targetPosition = target.position + targetOffset;

        // Calcular posicao desejada com base na rotacao horizontal
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

        // SphereCast para detetar colisoes entre o alvo e a camera
        Vector3 direction = desiredPosition - targetPosition;
        RaycastHit hit;

        if (Physics.SphereCast(targetPosition, collisionRadius, direction.normalized, out hit, distance, collisionMask))
        {
            // Aproximar a camera ao ponto de colisao
            transform.position = targetPosition + direction.normalized * (hit.distance - collisionRadius);
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(targetPosition);
    }
}
