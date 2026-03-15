using System.Collections;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [Header("Look At")]
    public Transform lookTarget;
    public float lookSpeed = 3f;

    [Header("Audio")]
    public AudioClip voiceLine;

    [Header("Referencias")]
    public FPMove playerMovement;
    public float slowMultiplier = 0.3f;
    public float lookDuration = 6f;

    private float origSpeed;
    private float origSprintSpeed;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // só funciona com as luzes ligadas
        if (DarknessManager.Instance != null && DarknessManager.Instance.IsDark()) return;

        Destroy(GetComponent<Collider>());
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        // abrandar o player e bloquear câmara
        if (playerMovement != null)
        {
            origSpeed = playerMovement.speed;
            origSprintSpeed = playerMovement.sprintSpeed;
            playerMovement.speed = origSpeed * slowMultiplier;
            playerMovement.sprintSpeed = origSprintSpeed * slowMultiplier;
            playerMovement.cameraBlocked = true;
        }

        // tocar audio imediatamente
        AudioSource audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (voiceLine != null)
            audio.PlayOneShot(voiceLine);

        // durante 6 segundos — forçar câmara a olhar para o target
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
                // -- rotação horizontal do corpo --
                Vector3 dir = lookTarget.position - playerT.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    playerT.rotation = Quaternion.Slerp(
                        playerT.rotation,
                        Quaternion.LookRotation(dir),
                        Time.deltaTime * lookSpeed);

                // -- rotação vertical da câmara --
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

        // devolver velocidade e desbloquear câmara
        if (playerMovement != null)
        {
            playerMovement.speed = origSpeed;
            playerMovement.sprintSpeed = origSprintSpeed;
            playerMovement.cameraBlocked = false;
            playerMovement.SyncCameraRotation(); // evitar salto brusco
        }

        // aguardar o resto do audio
        if (voiceLine != null)
            yield return new WaitForSeconds(Mathf.Max(0f, voiceLine.length - lookDuration));

        GameStateManager.Instance?.PopState();
        Destroy(gameObject);
    }

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