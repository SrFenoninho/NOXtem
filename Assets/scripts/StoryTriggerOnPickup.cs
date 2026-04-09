using System.Collections;
using UnityEngine;

public class StoryTriggerOnPickup : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Monitoring object")]
    public GameObject watchObject; // o objeto da chave - quando for destruido dispara

    [Header("Look At")]
    public Transform lookTarget;
    public float lookSpeed = 3f;

    [Header("Audio")]
    public AudioClip voiceLine;

    [Header("References")]
    public FPMove playerMovement;
    public float slowMultiplier = 0f; // 0 = velocidade normal
    public float lookDuration = 4f;
    public bool triggerInDarkZone = false;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float origSpeed;
    private float origSprintSpeed;
    private bool triggered = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (triggered) return;

        if (watchObject == null)
        {
            triggered = true;

            if (!triggerInDarkZone && DarknessManager.Instance != null && DarknessManager.Instance.IsDark()) return;

            StartCoroutine(RunSequence());
        }
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

        Camera cam = playerMovement != null ? playerMovement.GetComponentInChildren<Camera>() : null;
        Transform camT = cam != null ? cam.transform : null;

        while (elapsed < lookDuration)
        {
            elapsed += Time.deltaTime;

            if (lookTarget != null && camT != null)
            {
                Vector3 dirCam = lookTarget.position - camT.position;
                float pitch = -Mathf.Asin(Mathf.Clamp(dirCam.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;

                camT.localRotation = Quaternion.Slerp(
                    camT.localRotation,
                    Quaternion.Euler(pitch, 0f, 0f),
                    Time.deltaTime * lookSpeed);
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
    }
}