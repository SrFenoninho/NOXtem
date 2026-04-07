using System.Collections;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Look At")]
    public Transform lookTarget;
    public float lookSpeed = 3f;

    [Header("Audio")]
    public AudioClip voiceLine;

    [Header("Referencias")]
    public FPMove playerMovement;
    public float slowMultiplier = 0f; // 0 = velocidade normal, sem alteracao
    public float lookDuration = 6f;
    public bool triggerInDarkZone = false;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float origSpeed;
    private float origSprintSpeed;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!triggerInDarkZone && DarknessManager.Instance != null && DarknessManager.Instance.IsDark()) return;

        Destroy(GetComponent<Collider>());
        StartCoroutine(RunSequence());
    }

    // ---------------------------------------------
    //  SEQUENCIA
    // ---------------------------------------------
    IEnumerator RunSequence()
    {
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        if (playerMovement != null)
        {
            origSpeed = playerMovement.speed;
            origSprintSpeed = playerMovement.sprintSpeed;
            if (slowMultiplier > 0f)
            {
                playerMovement.speed = origSpeed * slowMultiplier;
                playerMovement.sprintSpeed = origSprintSpeed * slowMultiplier;
            }
            if (lookTarget != null) playerMovement.cameraBlocked = true;
        }

        AudioSource audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (voiceLine != null)
            audio.PlayOneShot(voiceLine);

        float elapsed = 0f;
        float lookDuration = 6f;

        Transform playerT = playerMovement != null ? playerMovement.transform : null;
        Camera cam = playerMovement != null ? playerMovement.GetComponentInChildren<Camera>() : null;
        Transform camT = cam != null ? cam.transform : null;

        while (elapsed < lookDuration)
        {
            elapsed += Time.deltaTime;

            if (lookTarget != null && playerT != null)
            {
                Vector3 dir = lookTarget.position - playerT.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    playerT.rotation = Quaternion.Slerp(
                        playerT.rotation,
                        Quaternion.LookRotation(dir),
                        Time.deltaTime * lookSpeed);

                if (camT != null)
                {
                    Vector3 dirCam = lookTarget.position - camT.position;
                    float pitch = -Mathf.Asin(Mathf.Clamp(dirCam.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;

                    camT.localRotation = Quaternion.Slerp(
                        camT.localRotation,
                        Quaternion.Euler(pitch, 0f, 0f),
                        Time.deltaTime * lookSpeed);
                }
            }

            yield return null;
        }

        if (playerMovement != null)
        {
            if (slowMultiplier > 0f)
            {
                playerMovement.speed = origSpeed;
                playerMovement.sprintSpeed = origSprintSpeed;
            }
            if (lookTarget != null)
            {
                playerMovement.cameraBlocked = false;
                playerMovement.SyncCameraRotation();
            }
        }

        if (voiceLine != null)
            yield return new WaitForSeconds(Mathf.Max(0f, voiceLine.length - lookDuration));

        GameStateManager.Instance?.PopState();
        Destroy(gameObject);
    }

    // ---------------------------------------------
    //  DESENHOS DE DEPURACAO
    // ---------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (lookTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, lookTarget.position);
            Gizmos.DrawSphere(lookTarget.position, 0.2f);
        }
    }
}