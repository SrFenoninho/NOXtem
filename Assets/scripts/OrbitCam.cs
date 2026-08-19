using UnityEngine;

public class OrbitCam : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Target")]

    public Transform target;

    public Vector3 targetOffset = new Vector3(0, 1.6f, 0);

    [Header("Camera Settings")]
    public float distance = 5f;
    public float sensitivity = 100f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float collisionRadius = 0.3f;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private float yaw = 0f;
    private float pitch = 20f;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }

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
    //  PRIVATE METHODS
    // ---------------------------------------------
    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * (sensitivity * 0.05f);
        yaw += mouseX;
    }

    void UpdateCameraPosition()
    {
        Vector3 targetPosition = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = targetPosition - rotation * Vector3.forward * distance;

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

        transform.LookAt(targetPosition);
    }
}
